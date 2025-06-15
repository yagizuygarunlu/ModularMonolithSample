using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolithSample.Event.Application.Commands.CreateEvent;
using ModularMonolithSample.Event.Domain;

namespace ModularMonolithSample.Event.Infrastructure;

public static class EventModuleConfiguration
{
    public static IServiceCollection AddEventModule(this IServiceCollection services)
    {
        services.AddDbContext<EventDbContext>(options =>
            options.UseInMemoryDatabase("EventDb"));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateEventCommand).Assembly));

        return services;
    }
} 