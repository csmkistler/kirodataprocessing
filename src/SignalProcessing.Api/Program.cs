var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on HTTP only for local development
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // HTTP
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Core services (will be implemented in later tasks)
// builder.Services.AddScoped<ISignalGenerator, SignalGenerator>();
// builder.Services.AddScoped<ISignalProcessor, SignalProcessor>();
// builder.Services.AddScoped<ITriggerComponent, TriggerComponent>();

// Application services (will be implemented in later tasks)
// builder.Services.AddScoped<SignalGenerationService>();
// builder.Services.AddScoped<SignalProcessingService>();
// builder.Services.AddScoped<TriggerService>();

// Infrastructure services (will be implemented in later tasks)
// builder.Services.AddSingleton<ITimeSeriesDatabase, InfluxDbAdapter>();
// builder.Services.AddSingleton<IMetadataDatabase, MongoDbAdapter>();
// builder.Services.AddScoped<ISignalDatabase, SignalDatabase>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
