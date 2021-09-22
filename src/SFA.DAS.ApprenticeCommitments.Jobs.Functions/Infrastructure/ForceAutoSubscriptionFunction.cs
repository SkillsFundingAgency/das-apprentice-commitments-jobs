using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure
{
    public class ForceAutoEventSubscription : IMessage { }

    public class ForceAutoEventSubscriptionFunction
    {
        private readonly IFunctionEndpoint functionEndpoint;

        public ForceAutoEventSubscriptionFunction(IFunctionEndpoint functionEndpoint)
            => this.functionEndpoint = functionEndpoint;

        [FunctionName("ForceAutoSubscriptionFunction")]
        public async Task Run(
            [TimerTrigger("* * * 1 1 *", RunOnStartup = true)] TimerInfo myTimer,
            ILogger logger, ExecutionContext executionContext)
        {
            var sendOptions = new SendOptions();
            sendOptions.SetHeader(Headers.ControlMessageHeader, bool.TrueString);
            sendOptions.SetHeader(Headers.MessageIntent, nameof(MessageIntentEnum.Send));
            sendOptions.RouteToThisEndpoint();
            await functionEndpoint.Send(new ForceAutoEventSubscription(), sendOptions, executionContext, logger);
        }
    }
}