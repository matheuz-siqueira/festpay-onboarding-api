using Festpay.Onboarding.Application.Common.Behaviours;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Festpay.Onboarding.Application.Modules;

public static class AppModules
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(AppModules).Assembly);
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(AppModules).Assembly);

            options.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });
    }
}
