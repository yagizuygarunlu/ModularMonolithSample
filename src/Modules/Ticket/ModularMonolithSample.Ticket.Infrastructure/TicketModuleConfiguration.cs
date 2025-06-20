using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolithSample.BuildingBlocks.Behaviors;
using ModularMonolithSample.BuildingBlocks.Common;
using ModularMonolithSample.BuildingBlocks.Infrastructure;
using ModularMonolithSample.Ticket.Application.Commands.IssueTicket;
using ModularMonolithSample.Ticket.Application.EventHandlers;
using ModularMonolithSample.Ticket.Domain;

namespace ModularMonolithSample.Ticket.Infrastructure;

public static class TicketModuleConfiguration
{
    public static IServiceCollection AddTicketModule(this IServiceCollection services)
    {
        services.AddDbContext<TicketDbContext>(options =>
            options.UseInMemoryDatabase("TicketDb"));

        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        // Add validators
        services.AddValidatorsFromAssembly(typeof(IssueTicketCommand).Assembly);
        
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(IssueTicketCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(AttendeeRegisteredDomainEventHandler).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
} 