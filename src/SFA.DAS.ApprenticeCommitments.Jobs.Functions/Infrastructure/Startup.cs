using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;
using RestEase.HttpClientFactory;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.Http.Configuration;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

            CreateQueuesWithReflection(
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

                configuration.Transport.SubscriptionRuleNamingConvention(SFA.DAS.NServiceBus.Configuration.AzureServiceBus.RuleNameShortener.Shorten);

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

        private static async Task CreateQueuesWithReflection(
            IConfiguration configuration,
            string? auditQueue = null,
            string? errorQueue = null,
            string? topicName = "bundle-1",
            string connectionStringName = "AzureWebJobsServiceBus",
            ILogger? logger = null)
        {
            var connectionString = configuration.GetValue<string>(connectionStringName);
            var managementClient = new ManagementClient(connectionString);

            var attribute = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttribute<FunctionNameAttribute>(false) != null)
                .SelectMany(m => m.GetParameters())
                .SelectMany(p => p.GetCustomAttributes<ServiceBusTriggerAttribute>(false))
                .FirstOrDefault()
                ?? throw new Exception("No endpoint was found");

            var endpointQueueName = attribute.QueueName;

            logger?.LogInformation("Queue Name: {queueName}", endpointQueueName);

            auditQueue ??= $"{endpointQueueName}-audit";
            errorQueue ??= $"{endpointQueueName}-error";

            await CreateQueue(endpointQueueName, managementClient, logger);
            //await CreateQueue(auditQueue, managementClient, logger);
            await CreateQueue(errorQueue, managementClient, logger);

            await CreateSubscription(topicName, managementClient, endpointQueueName);
        }

        private static async Task CreateQueue(string endpointQueueName, ManagementClient managementClient, ILogger? logger)
        {
            if (!await managementClient.QueueExistsAsync(endpointQueueName))
            {
                logger?.LogInformation("Creating queue: `{queueName}`", endpointQueueName);
                await managementClient.CreateQueueAsync(endpointQueueName);
            }
        }

        private static async Task CreateSubscription(string topicName, ManagementClient managementClient, string endpointQueueName)
        {
            if (!await managementClient.SubscriptionExistsAsync(topicName, endpointQueueName))
            {
                var subscriptionDescription = new SubscriptionDescription(topicName, endpointQueueName)
                {
                    ForwardTo = endpointQueueName,
                    UserMetadata = $"Events {endpointQueueName} subscribed to"
                };
                var ruleDescription = new RuleDescription
                {
                    Filter = new FalseFilter()
                };
                await managementClient.CreateSubscriptionAsync(subscriptionDescription, ruleDescription);
            }
        }
    }

    public class ForceAutoSubscription : IMessage { }

    public class TimerFunc
    {
        private readonly IFunctionEndpoint functionEndpoint;

        public TimerFunc(IFunctionEndpoint functionEndpoint)
        {
            this.functionEndpoint = functionEndpoint;
        }

        [FunctionName("TimerFunc")]
        public async Task Run([TimerTrigger("* * * 1 1 *", RunOnStartup = true)] TimerInfo myTimer,
            ILogger logger, ExecutionContext executionContext)
        {
            var sendOptions = new SendOptions();
            sendOptions.SetHeader(Headers.ControlMessageHeader, bool.TrueString);
            sendOptions.SetHeader(Headers.MessageIntent, MessageIntentEnum.Send.ToString());
            sendOptions.RouteToThisEndpoint();
            await functionEndpoint.Send(new ForceAutoSubscription(), sendOptions, executionContext, logger);
        }
    }
}

namespace SFA.DAS.NServiceBus.Configuration.AzureServiceBus
{
    public static class RuleNameShortener
    {
        private const int AzureServiceBusRuleNameMaxLength = 50;

        public static string Shorten(Type rule)
        {
            var ruleName = rule.FullName;
            var importantName = ruleName.Replace("SFA.DAS.", "").Replace(".Messages.Events", "");

            if (importantName.Length <= AzureServiceBusRuleNameMaxLength)
                return importantName;

            using var md5 = new MD5CryptoServiceProvider();
            var bytes = Encoding.Default.GetBytes(ruleName);
            var hash = md5.ComputeHash(bytes);
            var hashstr = BitConverter.ToString(hash).Replace("-", string.Empty);

            var shortName = $"{hashstr.Substring(0, 8)}{importantName[^42..]}";
            return shortName;
        }
    }
}