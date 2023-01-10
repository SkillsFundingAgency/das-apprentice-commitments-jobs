using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure
{
    internal class AzureServiceBusTriggerFunction
    {
        private const string EndpointName = QueueNames.ApprenticeshipCommitmentsJobs;
        private readonly IFunctionEndpoint endpoint;

        public AzureServiceBusTriggerFunction(IFunctionEndpoint endpoint) => this.endpoint = endpoint;

        [FunctionName("ApprenticeshipCommitmentsJobs")]
        public async Task Run(
            [ServiceBusTrigger(queueName: EndpointName, Connection = "NServiceBusConnectionString")] ServiceBusReceivedMessage message,
            ServiceBusClient client, ServiceBusMessageActions messageActions,
            ILogger logger,
            ExecutionContext context)
        {
            //await endpoint.ProcessAtomic(message, context, client, messageActions, logger);
            await endpoint.ProcessNonAtomic(message, context, logger);
        }
    }
}