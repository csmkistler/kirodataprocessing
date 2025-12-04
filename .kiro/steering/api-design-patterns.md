---
inclusion: always
---

# API Design Patterns

## RESTful API Conventions

### Resource Naming
- Use plural nouns for collections: `/api/signals`, `/api/events`
- Use specific IDs for individual resources: `/api/signals/{id}`
- Use nested routes for related resources: `/api/signals/{id}/process`
- Use kebab-case for multi-word resources: `/api/trigger-events`

### HTTP Methods
- `GET` - Retrieve resource(s)
- `POST` - Create new resource
- `PUT` - Update entire resource
- `PATCH` - Partial update
- `DELETE` - Remove resource

### Status Codes
- `200 OK` - Successful GET, PUT, PATCH
- `201 Created` - Successful POST
- `204 No Content` - Successful DELETE
- `400 Bad Request` - Invalid input
- `404 Not Found` - Resource doesn't exist
- `500 Internal Server Error` - Server error

## Controller Structure

### Minimal Controller Pattern
```csharp
[ApiController]
[Route("api/[controller]")]
public class SignalsController : ControllerBase
{
    private readonly SignalGenerationService _generationService;
    private readonly ILogger<SignalsController> _logger;

    public SignalsController(
        SignalGenerationService generationService,
        ILogger<SignalsController> logger)
    {
        _generationService = generationService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SignalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SignalDto>> GetSignal(Guid id)
    {
        var signal = await _generationService.GetSignalAsync(id);
        if (signal == null)
            return NotFound();

        return Ok(signal);
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(SignalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SignalDto>> GenerateSignal(
        [FromBody] SignalGenerationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var signal = await _generationService.GenerateSignalAsync(request);
        return CreatedAtAction(nameof(GetSignal), new { id = signal.Id }, signal);
    }
}
```

## Request/Response Models

### DTOs (Data Transfer Objects)
```csharp
// Request DTO
public record SignalGenerationRequest(
    [Required] SignalType Type,
    [Range(0.1, 20000)] double Frequency,
    [Range(0.01, 100)] double Amplitude,
    [Range(-6.28, 6.28)] double Phase,
    [Range(0.1, 60)] double Duration,
    [Required] int SampleRate
);

// Response DTO
public record SignalDto(
    Guid Id,
    string Type,
    SignalMetadataDto Metadata,
    DateTime CreatedAt
);

public record SignalMetadataDto(
    double Frequency,
    double Amplitude,
    double Phase,
    double Duration,
    int SampleRate
);
```

### Standard Response Wrapper
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public List<string>? ValidationErrors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error
        };
    }
}
```

## Error Handling

### Global Exception Handler
```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var (statusCode, message) = exception switch
        {
            InvalidSignalParametersException => (StatusCodes.Status400BadRequest, exception.Message),
            SignalNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            DatabaseConnectionException => (StatusCodes.Status503ServiceUnavailable, "Database unavailable"),
            _ => (StatusCodes.Status500InternalServerError, "An internal error occurred")
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            error = message,
            statusCode
        }, cancellationToken);

        return true;
    }
}

// Register in Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
```

### Validation Error Response
```csharp
[HttpPost]
public async Task<ActionResult<SignalDto>> Create([FromBody] SignalGenerationRequest request)
{
    if (!ModelState.IsValid)
    {
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return BadRequest(new
        {
            error = "Validation failed",
            validationErrors = errors
        });
    }

    // Process request
}
```

## API Versioning

### URL Versioning
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class SignalsController : ControllerBase
{
    // v1 implementation
}

[ApiController]
[Route("api/v2/[controller]")]
public class SignalsV2Controller : ControllerBase
{
    // v2 implementation with breaking changes
}
```

## CORS Configuration

### Configure CORS in Program.cs
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDevelopment", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();
app.UseCors("LocalDevelopment");
```

## Logging

### Structured Logging
```csharp
[HttpPost("generate")]
public async Task<ActionResult<SignalDto>> GenerateSignal(
    [FromBody] SignalGenerationRequest request)
{
    _logger.LogInformation(
        "Generating signal: Type={Type}, Frequency={Frequency}",
        request.Type,
        request.Frequency
    );

    try
    {
        var signal = await _generationService.GenerateSignalAsync(request);
        
        _logger.LogInformation(
            "Signal generated successfully: Id={SignalId}",
            signal.Id
        );

        return CreatedAtAction(nameof(GetSignal), new { id = signal.Id }, signal);
    }
    catch (Exception ex)
    {
        _logger.LogError(
            ex,
            "Failed to generate signal: Type={Type}, Frequency={Frequency}",
            request.Type,
            request.Frequency
        );
        throw;
    }
}
```

## Pagination

### Pagination Parameters
```csharp
public record PaginationParams(
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, 100)] int PageSize = 10
);

public record PagedResult<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
```

### Paginated Endpoint
```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<SignalDto>>> GetSignals(
    [FromQuery] PaginationParams pagination)
{
    var (signals, totalCount) = await _generationService.GetSignalsAsync(
        pagination.Page,
        pagination.PageSize
    );

    var result = new PagedResult<SignalDto>(
        Items: signals,
        Page: pagination.Page,
        PageSize: pagination.PageSize,
        TotalCount: totalCount,
        TotalPages: (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
    );

    return Ok(result);
}
```

## API Documentation with Swagger

### Configure Swagger
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Signal Processing API",
        Version = "v1",
        Description = "API for signal generation, processing, and visualization"
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

### XML Documentation
```csharp
/// <summary>
/// Generates a new signal based on the provided parameters.
/// </summary>
/// <param name="request">The signal generation parameters.</param>
/// <returns>The generated signal.</returns>
/// <response code="201">Signal created successfully.</response>
/// <response code="400">Invalid parameters provided.</response>
[HttpPost("generate")]
[ProducesResponseType(typeof(SignalDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<SignalDto>> GenerateSignal(
    [FromBody] SignalGenerationRequest request)
{
    // Implementation
}
```

## Best Practices

1. **Keep controllers thin** - Business logic belongs in services
2. **Use DTOs** - Don't expose domain entities directly
3. **Validate input** - Use data annotations and FluentValidation
4. **Handle errors globally** - Use exception handlers
5. **Log appropriately** - Info for success, Error for failures
6. **Document APIs** - Use Swagger/OpenAPI
7. **Version APIs** - Plan for breaking changes
8. **Use async/await** - For all I/O operations
9. **Return appropriate status codes** - Follow HTTP standards
10. **Implement CORS** - For cross-origin requests
