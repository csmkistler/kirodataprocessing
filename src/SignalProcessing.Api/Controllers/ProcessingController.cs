using Microsoft.AspNetCore.Mvc;
using SignalProcessing.Api.Models;
using SignalProcessing.Application.DTOs;
using SignalProcessing.Application.Services;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Api.Controllers;

/// <summary>
/// Controller for signal processing operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProcessingController : ControllerBase
{
    private readonly SignalProcessingService _processingService;
    private readonly ILogger<ProcessingController> _logger;

    public ProcessingController(
        SignalProcessingService processingService,
        ILogger<ProcessingController> logger)
    {
        _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes a signal with the specified parameters.
    /// </summary>
    /// <param name="request">Processing parameters.</param>
    /// <returns>The processed signal.</returns>
    /// <response code="201">Signal processed successfully.</response>
    /// <response code="400">Invalid parameters provided.</response>
    /// <response code="404">Original signal not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("process")]
    [ProducesResponseType(typeof(ProcessedSignalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProcessedSignalDto>> ProcessSignal([FromBody] ProcessingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation(
            "Processing signal: SignalId={SignalId}, Operation={Operation}",
            request.SignalId,
            request.Operation
        );

        try
        {
            // Parse operation type
            if (!Enum.TryParse<OperationType>(request.Operation, true, out var operationType))
            {
                return BadRequest(new { error = $"Invalid operation type: {request.Operation}. Valid types are: LowPass, HighPass, BandPass, Gain" });
            }

            // Create parameters
            var parameters = new ProcessingParams(
                Operation: operationType,
                CutoffFrequency: request.CutoffFrequency,
                LowCutoff: request.LowCutoff,
                HighCutoff: request.HighCutoff,
                Gain: request.Gain,
                Order: request.Order
            );

            // Process signal
            var processedSignal = await _processingService.ProcessSignalAsync(request.SignalId, parameters);

            _logger.LogInformation(
                "Signal processed successfully: Id={ProcessedSignalId}, OriginalId={OriginalSignalId}",
                processedSignal.Id,
                processedSignal.OriginalSignalId
            );

            return CreatedAtAction(
                nameof(GetProcessedSignal),
                new { id = processedSignal.Id },
                processedSignal
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(
                ex,
                "Signal not found: SignalId={SignalId}",
                request.SignalId
            );
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to process signal: SignalId={SignalId}, Operation={Operation}",
                request.SignalId,
                request.Operation
            );
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error processing signal: SignalId={SignalId}, Operation={Operation}",
                request.SignalId,
                request.Operation
            );
            return StatusCode(500, new { error = "An internal error occurred while processing the signal" });
        }
    }

    /// <summary>
    /// Retrieves a processed signal by its ID.
    /// </summary>
    /// <param name="id">Processed signal ID.</param>
    /// <returns>The processed signal with the specified ID.</returns>
    /// <response code="200">Processed signal found and returned.</response>
    /// <response code="404">Processed signal not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProcessedSignalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProcessedSignalDto>> GetProcessedSignal(Guid id)
    {
        _logger.LogInformation("Retrieving processed signal: Id={ProcessedSignalId}", id);

        var processedSignal = await _processingService.GetProcessedSignalAsync(id);
        
        if (processedSignal == null)
        {
            _logger.LogWarning("Processed signal not found: Id={ProcessedSignalId}", id);
            return NotFound(new { error = $"Processed signal with ID {id} not found" });
        }

        return Ok(processedSignal);
    }
}
