using ModularMonolithSample.Attendee.Infrastructure;
using ModularMonolithSample.Event.Infrastructure;
using ModularMonolithSample.Feedback.Infrastructure;
using ModularMonolithSample.Ticket.Infrastructure;

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

// Configure modules
builder.Services.AddEventModule();
builder.Services.AddAttendeeModule();
builder.Services.AddTicketModule();
builder.Services.AddFeedbackModule();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
