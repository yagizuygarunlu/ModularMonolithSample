using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolithSample.BuildingBlocks.Behaviors;
using ModularMonolithSample.BuildingBlocks.Common;
using ModularMonolithSample.BuildingBlocks.Infrastructure;
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
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        // Add validators
        services.AddValidatorsFromAssembly(typeof(CreateEventCommand).Assembly);
        
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateEventCommand).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
} 