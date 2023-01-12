using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using RestEase.HttpClientFactory;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.Http.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Extensions;
using SFA.DAS.NServiceBus.Extensions;

[assembly: FunctionsStartup(typeof(SFA.DAS.ApprenticeCommitments.Jobs.Functions.Startup))]

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class Startup : FunctionsStartup
    {
        public IConfiguration? Configuration { get; set; }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigureConfiguration();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            Configuration = builder.GetContext().Configuration;
            var useManagedIdentity = !Configuration.IsLocalAcceptanceOrDev();

            builder.Services.AddLogging();
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddApplicationOptions();
            builder.Services.ConfigureFromOptions(f => f.ApprenticeCommitmentsApi);
            builder.Services.AddSingleton<IApimClientConfiguration>(x => x.GetRequiredService<ApprenticeCommitmentsApiOptions>());

            InitialiseNServiceBus();

            builder.UseNServiceBus((IConfiguration appConfiguration) =>
            {
                var configuration = ServiceBusEndpointFactory.CreateSingleQueueConfiguration(QueueNames.ApprenticeshipCommitmentsJobs, appConfiguration, useManagedIdentity);
                configuration.AdvancedConfiguration.UseNewtonsoftJsonSerializer();
                configuration.AdvancedConfiguration.UseMessageConventions();
                configuration.AdvancedConfiguration.EnableInstallers();
                return configuration;
            });

            builder.Services.AddSingleton<IApimClientConfiguration>(x => x.GetRequiredService<ApprenticeCommitmentsApiOptions>());
            builder.Services.AddTransient<Http.MessageHandlers.DefaultHeadersHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.LoggingMessageHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.ApimHeadersHandler>();
            builder.Services.AddTransient<EmailService>();

            var url = builder.Services
                .BuildServiceProvider()
                .GetRequiredService<ApprenticeCommitmentsApiOptions>()
                .ApiBaseUrl;

            builder.Services.AddRestEaseClient<IEcsApi>(url)
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();
        }

        public void InitialiseNServiceBus()
        {
            var m = new NServiceBusResourceManager(Configuration, !Configuration.IsLocalAcceptanceOrDev());
            m.CreateWorkAndErrorQueues(QueueNames.ApprenticeshipCommitmentsJobs).GetAwaiter().GetResult();
            m.SubscribeToTopicForQueue(typeof(Startup).Assembly, QueueNames.ApprenticeshipCommitmentsJobs).GetAwaiter().GetResult();
        }
    }
}
