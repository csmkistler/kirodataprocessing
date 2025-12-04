using System.Reflection;
using SignalProcessing.Api.Middleware;
using SignalProcessing.Application.Services;
using SignalProcessing.Core.Interfaces;
using SignalProcessing.Infrastructure;
using SignalProcessing.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on HTTP only for local development
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // HTTP
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with XML documentation
builder.Services.AddSwaggerGen(options =>
{
    // Include XML comments for better API documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS for local development
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

// Register global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Core services
builder.Services.AddScoped<ISignalGenerator, SignalGenerator>();
builder.Services.AddScoped<ISignalProcessor, SignalProcessor>();
builder.Services.AddScoped<ITriggerComponent, TriggerComponent>();

// Application services
builder.Services.AddScoped<SignalGenerationService>();
builder.Services.AddScoped<SignalProcessingService>();
builder.Services.AddScoped<TriggerService>();

// Infrastructure services - Database
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<InfluxDbContext>();
builder.Services.AddSingleton<ITimeSeriesDatabase, InfluxTimeSeriesDatabase>();
builder.Services.AddSingleton<IMetadataDatabase, MongoMetadataDatabase>();
builder.Services.AddScoped<ISignalDatabase, SignalDatabase>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Signal Processing API v1");
        options.RoutePrefix = "swagger";
    });
}

// Use exception handler
app.UseExceptionHandler();

app.UseCors("LocalDevelopment");
// Disable HTTPS redirection for local development
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 Signal Processing API is running!");
Console.WriteLine("📍 API: http://localhost:5000");
Console.WriteLine("📚 Swagger: http://localhost:5000/swagger");
Console.WriteLine("💚 Health: http://localhost:5000/api/health");

app.Run();
