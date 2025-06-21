using ModularMonolithSample.Attendee.Infrastructure;
using ModularMonolithSample.Event.Infrastructure;
using ModularMonolithSample.Feedback.Infrastructure;
using ModularMonolithSample.Ticket.Infrastructure;
using ModularMonolithSample.BuildingBlocks.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Event Management API", 
        Version = "v1",
        Description = "A Modular Monolith Event Management System demonstrating DDD, CQRS, and Domain Events"
    });
});

// Add standard API services (exception handling + response wrapper)
builder.Services.AddBuildingBlocks(config =>
{
    config.EnableLogging = true;
    config.EnablePerformanceMonitoring = true;
    config.EnableCaching = true;
    config.EnableRetry = false; // Can be enabled for specific scenarios
    config.EnableAuditing = false; // Enable in production for compliance
    config.EnableTransactions = false; // Enable when you have proper transaction manager
    config.PerformanceSettings.SlowRequestThresholdMs = 2000;
    config.PerformanceSettings.MediumRequestThresholdMs = 500;
});

// Configure modules
builder.Services.AddEventModule();
builder.Services.AddAttendeeModule();
builder.Services.AddTicketModule();
builder.Services.AddFeedbackModule();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Global exception handling should be one of the first middlewares
app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Management API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at apps root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
