using FinitiGlossary.Application.Aggregators.AdminHistory;
using FinitiGlossary.Application.Aggregators.AdminView;
using FinitiGlossary.Application.Interfaces.Agregator;
using FinitiGlossary.Application.Interfaces.Auth;
using FinitiGlossary.Application.Interfaces.Term.Admin;
using FinitiGlossary.Application.Interfaces.Term.Public;
using FinitiGlossary.Application.Services.Auth;
using FinitiGlossary.Application.Services.Term.Admin;
using FinitiGlossary.Application.Services.Term.Public;
using Microsoft.Extensions.DependencyInjection;

namespace FinitiGlossary.Application.DependencyInjection
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminGlossaryService, AdminGlossaryService>();
            services.AddTransient<IGlossaryAdminViewAggregator, GlossaryAdminViewAggregator>();
            services.AddTransient<IGlossaryAdminViewHistoryAggregator, GlossaryAdminViewHistoryAggregator>();
            services.AddTransient<IPublicGlossaryService, PublicGlossaryService>();

            return services;
        }
    }
}