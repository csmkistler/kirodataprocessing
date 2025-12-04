using Microsoft.AspNetCore.Mvc;
using SignalProcessing.Api.Models;
using SignalProcessing.Application.DTOs;
using SignalProcessing.Application.Services;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Api.Controllers;

/// <summary>
/// Controller for signal generation and retrieval operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SignalsController : ControllerBase
{
    private readonly SignalGenerationService _generationService;
    private readonly ILogger<SignalsController> _logger;

    public SignalsController(
        SignalGenerationService generationService,
        ILogger<SignalsController> logger)
    {
        _generationService = generationService ?? throw new ArgumentNullException(nameof(generationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates a new signal based on the provided parameters.
    /// </summary>
    /// <param name="request">Signal generation parameters.</param>
    /// <returns>The generated signal.</returns>
    /// <response code="201">Signal created successfully.</response>
    /// <response code="400">Invalid parameters provided.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(SignalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SignalDto>> GenerateSignal([FromBody] SignalGenerationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation(
            "Generating signal: Type={Type}, Frequency={Frequency}",
            request.Type,
            request.Frequency
        );

        try
        {
            // Parse signal type
            if (!Enum.TryParse<SignalType>(request.Type, true, out var signalType))
            {
                return BadRequest(new { error = $"Invalid signal type: {request.Type}. Valid types are: Sine, Square, Sawtooth, Noise" });
            }

            // Create parameters
            var parameters = new SignalGeneratorParams(
                Type: signalType,
                Frequency: request.Frequency,
                Amplitude: request.Amplitude,
                Phase: request.Phase,
                Duration: request.Duration,
                SampleRate: request.SampleRate
            );

            // Generate signal
            var signal = await _generationService.GenerateSignalAsync(parameters);

            _logger.LogInformation(
                "Signal generated successfully: Id={SignalId}",
                signal.Id
            );

            return CreatedAtAction(
                nameof(GetSignal),
                new { id = signal.Id },
                signal
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to generate signal: Type={Type}, Frequency={Frequency}",
                request.Type,
                request.Frequency
            );
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error generating signal: Type={Type}, Frequency={Frequency}",
                request.Type,
                request.Frequency
            );
            return StatusCode(500, new { error = "An internal error occurred while generating the signal" });
        }
    }

    /// <summary>
    /// Retrieves a signal by its ID.
    /// </summary>
    /// <param name="id">Signal ID.</param>
    /// <returns>The signal with the specified ID.</returns>
    /// <response code="200">Signal found and returned.</response>
    /// <response code="404">Signal not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SignalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SignalDto>> GetSignal(Guid id)
    {
        _logger.LogInformation("Retrieving signal: Id={SignalId}", id);

        var signal = await _generationService.GetSignalAsync(id);
        
        if (signal == null)
        {
            _logger.LogWarning("Signal not found: Id={SignalId}", id);
            return NotFound(new { error = $"Signal with ID {id} not found" });
        }

        return Ok(signal);
    }

    /// <summary>
    /// Retrieves recent signals with pagination.
    /// </summary>
    /// <param name="limit">Maximum number of signals to retrieve (default: 10, max: 100).</param>
    /// <returns>List of recent signals.</returns>
    /// <response code="200">Signals retrieved successfully.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<SignalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<SignalDto>>> GetSignals([FromQuery] int limit = 10)
    {
        if (limit < 1 || limit > 100)
        {
            return BadRequest(new { error = "Limit must be between 1 and 100" });
        }

        _logger.LogInformation("Retrieving recent signals: Limit={Limit}", limit);

        try
        {
            var signals = await _generationService.GetRecentSignalsAsync(limit);
            return Ok(signals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving signals");
            return StatusCode(500, new { error = "An internal error occurred while retrieving signals" });
        }
    }
}
