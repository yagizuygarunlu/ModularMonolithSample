using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;
using ModularMonolithSample.Attendee.Application.EventHandlers;
using ModularMonolithSample.Attendee.Domain;
using ModularMonolithSample.BuildingBlocks.Behaviors;
using ModularMonolithSample.BuildingBlocks.Common;
using ModularMonolithSample.BuildingBlocks.Infrastructure;

namespace ModularMonolithSample.Attendee.Infrastructure;

public static class AttendeeModuleConfiguration
{
    public static IServiceCollection AddAttendeeModule(this IServiceCollection services)
    {
        services.AddDbContext<AttendeeDbContext>(options =>
            options.UseInMemoryDatabase("AttendeeDatabase"));

        services.AddScoped<IAttendeeRepository, AttendeeRepository>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        // Add validators
        services.AddValidatorsFromAssembly(typeof(RegisterAttendeeCommand).Assembly);
        
        // MediatR configuration for version 11.1.0
        services.AddMediatR(typeof(RegisterAttendeeCommand).Assembly);
        services.AddMediatR(typeof(EventCreatedDomainEventHandler).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
} 