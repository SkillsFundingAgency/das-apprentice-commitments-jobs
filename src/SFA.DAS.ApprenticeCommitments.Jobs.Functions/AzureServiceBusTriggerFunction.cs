using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    internal class AzureServiceBusTriggerFunction
    {
        private const string EndpointName = QueueNames.ApprenticeshipCreatedEvent;
        private readonly IFunctionEndpoint endpoint;

        public AzureServiceBusTriggerFunction(IFunctionEndpoint endpoint) => this.endpoint = endpoint;

        [FunctionName("ApprenticeshipCreatedEvent")]
        public async Task Run(
            [ServiceBusTrigger(queueName: EndpointName)] Message message,
            ExecutionContext context)
        {
            await endpoint.Process(message, context);
        }
    }
}