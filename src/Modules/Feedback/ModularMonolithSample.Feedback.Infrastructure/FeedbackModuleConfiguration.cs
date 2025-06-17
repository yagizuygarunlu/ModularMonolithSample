using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SubmitFeedbackCommand).Assembly));

        return services;
    }
} 