using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using System.IO;
using System.Reflection;

[assembly: FunctionsStartup(typeof(SFA.DAS.ApprenticeCommitments.Jobs.Startup))]

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.ConfigureLogging();
            builder.ConfigureConfiguration();
        }
    }

    internal static class StartupParts
    {
        internal static void ConfigureLogging(this IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(logBuilder =>
            {
                // all logging is filtered out by defualt
                logBuilder.AddFilter(typeof(Startup).Namespace, LogLevel.Information);
                var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
                var files = Directory.GetFiles(rootDirectory, "nlog.config", SearchOption.AllDirectories)[0];
                logBuilder.AddNLog(files);
            });
        }

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
                options.ConfigurationKeys = preConfig["ConfigurationNames"].Split(",");
                options.StorageConnectionString = preConfig["ConfigurationStorage"];
                options.EnvironmentName = preConfig["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
        }
    }
}