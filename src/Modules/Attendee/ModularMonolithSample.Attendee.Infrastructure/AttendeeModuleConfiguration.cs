using FluentValidation;
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
            options.UseInMemoryDatabase("AttendeeDb"));

        services.AddScoped<IAttendeeRepository, AttendeeRepository>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        // Add validators
        services.AddValidatorsFromAssembly(typeof(RegisterAttendeeCommand).Assembly);
        
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(RegisterAttendeeCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(EventCreatedDomainEventHandler).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
} 