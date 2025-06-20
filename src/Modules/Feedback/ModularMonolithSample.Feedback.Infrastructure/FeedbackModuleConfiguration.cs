using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolithSample.BuildingBlocks.Behaviors;
using ModularMonolithSample.BuildingBlocks.Common;
using ModularMonolithSample.BuildingBlocks.Infrastructure;
using ModularMonolithSample.Feedback.Application.Commands.SubmitFeedback;
using ModularMonolithSample.Feedback.Domain;

namespace ModularMonolithSample.Feedback.Infrastructure;

public static class FeedbackModuleConfiguration
{
    public static IServiceCollection AddFeedbackModule(this IServiceCollection services)
    {
        services.AddDbContext<FeedbackDbContext>(options =>
            options.UseInMemoryDatabase("FeedbackDb"));

        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        // Add validators
        services.AddValidatorsFromAssembly(typeof(SubmitFeedbackCommand).Assembly);
        
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(SubmitFeedbackCommand).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
} 