using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure
{
    internal static class EsfaConfigurationExtension
    {
        internal static void ConfigureConfiguration(this IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigurationBuilder.AddJsonFile("local.settings.json", optional: true);

            var preConfig = builder.ConfigurationBuilder.Build();

            builder.ConfigurationBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = preConfig["ConfigNames"].Split(",");
                options.StorageConnectionString = preConfig["ConfigurationStorageConnectionString"];
                options.EnvironmentName = preConfig["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });
        }

        public static void ConfigureOptions<TOptions>(this IServiceCollection services, string name)
            where TOptions : class
        {
            services
                .AddOptions<TOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                    configuration.Bind(name, settings));
        }

        public static void ConfigureFromOptions<TInterface, TOptions>(this IServiceCollection services)
            where TInterface : class
            where TOptions : class, TInterface, new()
        {
            services.AddSingleton<TInterface>(s =>
                s.GetRequiredService<IOptions<TOptions>>().Value);
        }
    }
}