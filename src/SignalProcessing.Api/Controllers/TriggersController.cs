using Microsoft.AspNetCore.Mvc;
using SignalProcessing.Api.Models;
using SignalProcessing.Application.DTOs;
using SignalProcessing.Application.Services;

namespace SignalProcessing.Api.Controllers;

/// <summary>
/// Controller for trigger configuration and event management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TriggersController : ControllerBase
{
    private readonly TriggerService _triggerService;
    private readonly ILogger<TriggersController> _logger;

    public TriggersController(
        TriggerService triggerService,
        ILogger<TriggersController> logger)
    {
        _triggerService = triggerService ?? throw new ArgumentNullException(nameof(triggerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Configures the trigger threshold.
    /// </summary>
    /// <param name="request">Trigger configuration parameters.</param>
    /// <returns>Success confirmation.</returns>
    /// <response code="200">Threshold configured successfully.</response>
    /// <response code="400">Invalid parameters provided.</response>
    [HttpPost("configure")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult ConfigureThreshold([FromBody] TriggerConfigRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation(
            "Configuring trigger: Threshold={Threshold}, Enabled={Enabled}",
            request.Threshold,
            request.Enabled
        );

        try
        {
            _triggerService.ConfigureThreshold(request.Threshold, request.Enabled);

            return Ok(new
            {
                message = "Trigger configured successfully",
                threshold = request.Threshold,
                enabled = request.Enabled
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring trigger: {Message}", ex.Message);
            return StatusCode(500, new { 
                error = "An internal error occurred while configuring the trigger",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Checks a value against the configured threshold.
    /// </summary>
    /// <param name="request">Value to check.</param>
    /// <returns>Trigger event if threshold was exceeded, null otherwise.</returns>
    /// <response code="200">Value checked successfully.</response>
    /// <response code="400">Invalid parameters provided.</response>
    [HttpPost("check")]
    [ProducesResponseType(typeof(TriggerCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TriggerCheckResponse>> CheckValue([FromBody] TriggerCheckRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Checking trigger value: Value={Value}", request.Value);

        try
        {
            var triggerEvent = await _triggerService.CheckValueAsync(request.Value);

            if (triggerEvent != null)
            {
                _logger.LogInformation(
                    "Trigger threshold exceeded: Value={Value}, Threshold={Threshold}",
                    triggerEvent.Value,
                    triggerEvent.Threshold
                );

                return Ok(new TriggerCheckResponse
                {
                    Triggered = true,
                    Event = triggerEvent
                });
            }

            return Ok(new TriggerCheckResponse
            {
                Triggered = false,
                Event = null
            });
        }
        catch (MongoDB.Driver.MongoException ex)
        {
            _logger.LogError(ex, "MongoDB error checking trigger value: {Message}", ex.Message);
            return StatusCode(503, new { 
                error = "Database connection error. Please ensure MongoDB is running.",
                details = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking trigger value: {Message}", ex.Message);
            return StatusCode(500, new { 
                error = "An internal error occurred while checking the trigger value",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Retrieves recent trigger events.
    /// </summary>
    /// <param name="limit">Maximum number of events to retrieve (default: 100, max: 1000).</param>
    /// <returns>List of trigger events in reverse chronological order.</returns>
    /// <response code="200">Events retrieved successfully.</response>
    /// <response code="400">Invalid parameters provided.</response>
    [HttpGet("events")]
    [ProducesResponseType(typeof(List<TriggerEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<TriggerEventDto>>> GetEvents([FromQuery] int limit = 100)
    {
        if (limit < 1 || limit > 1000)
        {
            return BadRequest(new { error = "Limit must be between 1 and 1000" });
        }

        _logger.LogInformation("Retrieving trigger events: Limit={Limit}", limit);

        try
        {
            var events = await _triggerService.GetEventsAsync(limit);
            return Ok(events);
        }
        catch (MongoDB.Driver.MongoException ex)
        {
            _logger.LogError(ex, "MongoDB error retrieving trigger events: {Message}", ex.Message);
            return StatusCode(503, new { 
                error = "Database connection error. Please ensure MongoDB is running.",
                details = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trigger events: {Message}", ex.Message);
            return StatusCode(500, new { 
                error = "An internal error occurred while retrieving trigger events",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Clears all trigger events.
    /// </summary>
    /// <returns>Success confirmation.</returns>
    /// <response code="200">Events cleared successfully.</response>
    [HttpDelete("events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ClearEvents()
    {
        _logger.LogInformation("Clearing all trigger events");

        try
        {
            await _triggerService.ClearEventsAsync();
            return Ok(new { message = "All trigger events cleared successfully" });
        }
        catch (MongoDB.Driver.MongoException ex)
        {
            _logger.LogError(ex, "MongoDB error clearing trigger events: {Message}", ex.Message);
            return StatusCode(503, new { 
                error = "Database connection error. Please ensure MongoDB is running.",
                details = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing trigger events: {Message}", ex.Message);
            return StatusCode(500, new { 
                error = "An internal error occurred while clearing trigger events",
                details = ex.Message 
            });
        }
    }
}

/// <summary>
/// Response model for trigger check operation.
/// </summary>
public record TriggerCheckResponse
{
    /// <summary>
    /// Whether the threshold was exceeded.
    /// </summary>
    public bool Triggered { get; init; }

    /// <summary>
    /// Trigger event details if threshold was exceeded.
    /// </summary>
    public TriggerEventDto? Event { get; init; }
}
