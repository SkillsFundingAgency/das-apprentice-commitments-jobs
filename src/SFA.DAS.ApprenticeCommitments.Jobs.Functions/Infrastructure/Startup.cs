using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using RestEase.HttpClientFactory;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.Http.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;

[assembly: FunctionsStartup(typeof(SFA.DAS.ApprenticeCommitments.Jobs.Functions.Startup))]

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigureConfiguration();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.ConfigureLogging();

            var logger = LoggerFactory.Create(b => b.ConfigureLogging()).CreateLogger<Startup>();

            AutoSubscribeToQueues.CreateQueuesWithReflection(
                builder.GetContext().Configuration,
                connectionStringName: "NServiceBusConnectionString",
                logger: logger)
                .GetAwaiter().GetResult();

            builder.UseNServiceBus(() =>
            {
                var configuration = new ServiceBusTriggeredEndpointConfiguration(
                    endpointName: QueueNames.ApprenticeshipCommitmentsJobs,
                    connectionStringName: "NServiceBusConnectionString");

                configuration.AdvancedConfiguration.SendFailedMessagesTo($"{QueueNames.ApprenticeshipCommitmentsJobs}-error");
                configuration.LogDiagnostics();

                configuration.AdvancedConfiguration.Conventions()
                    .DefiningEventsAs(t => t.Namespace?.StartsWith("SFA.DAS.CommitmentsV2.Messages.Events") == true);

                configuration.Transport.SubscriptionRuleNamingConvention(AzureQueueNameShortener.Shorten);

                return configuration;
            });

            builder.Services.ConfigureApplicationOptions<ApplicationSettings>();
            builder.Services.ConfigureFromOptions(f => f.ApprenticeCommitmentsApi);
            builder.Services.ConfigureFromOptions<IApimClientConfiguration, ApprenticeCommitmentsApiOptions>();
            builder.Services.AddSingleton<IApimClientConfiguration>(x => x.GetRequiredService<ApprenticeCommitmentsApiOptions>());

            builder.Services.AddTransient<Http.MessageHandlers.DefaultHeadersHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.LoggingMessageHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.ApimHeadersHandler>();

            var url = builder.Services
                .BuildServiceProvider()
                .GetRequiredService<ApprenticeCommitmentsApiOptions>()
                .ApiBaseUrl;

            builder.Services.AddRestEaseClient<IEcsApi>(url)
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>()
                //.AddTypedClient<>
                ;
        }
    }
}