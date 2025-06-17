using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;
using ModularMonolithSample.Attendee.Domain;

namespace ModularMonolithSample.Attendee.Infrastructure;

public static class AttendeeModuleConfiguration
{
    public static IServiceCollection AddAttendeeModule(this IServiceCollection services)
    {
        services.AddDbContext<AttendeeDbContext>(options =>
            options.UseInMemoryDatabase("AttendeeDb"));

        services.AddScoped<IAttendeeRepository, AttendeeRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterAttendeeCommand).Assembly));

        return services;
    }
} 