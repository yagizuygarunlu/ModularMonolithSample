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
        
        // MediatR configuration for version 11.1.0
        services.AddMediatR(typeof(IssueTicketCommand).Assembly);
        services.AddMediatR(typeof(AttendeeRegisteredDomainEventHandler).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
} 