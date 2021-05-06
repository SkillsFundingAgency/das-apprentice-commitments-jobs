using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NServiceBus;
using RestEase.HttpClientFactory;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.Http.Configuration;

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

            builder.UseNServiceBus(() =>
            {
                var configuration = new ServiceBusTriggeredEndpointConfiguration(
                    endpointName: QueueNames.ApprenticeshipCreatedEvent,
                    connectionStringName: "NServiceBusConnectionString");

                configuration.AdvancedConfiguration.SendFailedMessagesTo($"{QueueNames.ApprenticeshipCreatedEvent}-error");
                configuration.LogDiagnostics();

                return configuration;
            });

            builder.Services.ConfigureOptions<ApprenticeCommitmentsApiOptions>(
                ApprenticeCommitmentsApiOptions.ApprenticeCommitmentsApi);
            builder.Services.ConfigureFromOptions<IApimClientConfiguration, ApprenticeCommitmentsApiOptions>();

            builder.Services.AddTransient<Http.MessageHandlers.DefaultHeadersHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.LoggingMessageHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.ApimHeadersHandler>();

            var url = builder.Services
                .BuildServiceProvider()
                .GetRequiredService<IOptions<ApprenticeCommitmentsApiOptions>>()
                .Value.ApiBaseUrl;

            builder.Services.AddRestEaseClient<IEcsApi>(url)
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>()
                //.AddTypedClient<>
                ;
        }
    }
}