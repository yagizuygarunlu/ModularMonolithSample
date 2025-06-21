using FluentValidation;
using MediatR;
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
        // Register Entity Framework
        services.AddDbContext<EventDbContext>(options =>
            options.UseInMemoryDatabase("EventDatabase"));

        // Register repositories
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Register MediatR for this module (compatible with 11.1.0)
        services.AddMediatR(typeof(CreateEventCommand).Assembly);

        // Add validators
        services.AddValidatorsFromAssembly(typeof(CreateEventCommand).Assembly);
        
        // MediatR pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
} 