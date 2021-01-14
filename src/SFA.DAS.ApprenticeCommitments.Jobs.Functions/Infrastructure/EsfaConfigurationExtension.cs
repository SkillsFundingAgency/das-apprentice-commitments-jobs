using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using System.IO;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure
{
    internal static class EsfaConfigurationExtension
    {
        internal static void ConfigureConfiguration(this IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            configBuilder.AddJsonFile("local.settings.json", optional: true);

            var preConfig = configBuilder.Build();

            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = preConfig["ConfigNames"].Split(",");
                options.StorageConnectionString = preConfig["ConfigurationStorageConnectionString"];
                options.EnvironmentName = preConfig["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
        }

        public static void ConfigureOptions<TOptions>(this IServiceCollection services, string name)
            where TOptions : class
        {
            services
                .AddOptions<TOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                    configuration.Bind(name, settings));
        }
    }
}