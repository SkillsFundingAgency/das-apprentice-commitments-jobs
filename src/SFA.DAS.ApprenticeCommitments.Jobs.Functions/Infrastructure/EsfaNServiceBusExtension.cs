using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System.IO;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure
{
    internal static class EsfaNServiceBusExtension
    {
        internal static void ConfigureNServiceBus(this IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider
                .GetService<IConfiguration>();
            var logger = serviceProvider
                .GetService<ILoggerProvider>()
                .CreateLogger(typeof(EsfaNServiceBusExtension).AssemblyQualifiedName);

            if (config["NServiceBusConnectionString"] == "UseDevelopmentStorage=true")
            {
                builder.Services.AddNServiceBus(logger, (options) =>
                {
                    options.EndpointConfiguration = (endpoint) =>
                    {
                        endpoint
                            .UseTransport<LearningTransport>()
                            .StorageDirectory(".learningtransport");
                        return endpoint;
                    };
                });
            }
            else
            {
                builder.Services.AddNServiceBus(logger);
            }
        }

        private static string RepositoryPath =>
            Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src"));
    }
}