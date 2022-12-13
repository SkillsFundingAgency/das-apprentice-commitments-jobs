using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestEase.HttpClientFactory;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.Http.Configuration;
using SFA.DAS.Http.TokenGenerators;
using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure
{
    internal static class ApprenticeCommitmentsApiClientConfiguration
    {
        public static IServiceCollection AddInnerApi(this IServiceCollection services)
        {
            services.AddTransient<Http.MessageHandlers.DefaultHeadersHandler>();
            services.AddTransient<Http.MessageHandlers.LoggingMessageHandler>();

            var builder = services.AddHttpClient("AccountsApi")
                .ConfigureHttpClient((sp, client) =>
                {
                    var apiOptions = sp.GetRequiredService<ApprenticeCommitmentsApiOptions>();
                    client.BaseAddress = new Uri(apiOptions.ApiBaseUrl);
                })
                .UseWithRestEaseClient<IEcsApi>()
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>();

            if (UseManagedIdentity())
            {
                services.AddTransient<IApimClientConfiguration, ApprenticeCommitmentsApiOptions>();
                services.AddTransient<IManagedIdentityTokenGenerator, ManagedIdentityTokenGenerator>();
                services.AddTransient<Http.MessageHandlers.ManagedIdentityHeadersHandler>();

                builder.AddHttpMessageHandler<Http.MessageHandlers.ManagedIdentityHeadersHandler>();
            }

            builder.AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();

            return services;
        }

        private static bool UseManagedIdentity()
        {
            string environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "";
            return !environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
        }
    }
}
