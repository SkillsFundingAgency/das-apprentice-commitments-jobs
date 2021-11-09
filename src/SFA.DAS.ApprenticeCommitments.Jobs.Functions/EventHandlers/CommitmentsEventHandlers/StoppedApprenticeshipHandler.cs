using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.CommitmentsEventHandlerss
{
    public class StoppedApprenticeshipHandler
        : IHandleMessages<ApprenticeshipStoppedEvent>
        , IHandleMessages<ApprenticeshipCreatedEvent>
    {
        private readonly IDurableClient _durableClient;

        public StoppedApprenticeshipHandler(IDurableClientFactory clientFactory, IConfiguration configuration)
            => _durableClient = clientFactory.CreateClient(new DurableClientOptions
            {
                TaskHub = configuration["TaskHub"],
            });

        public async Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
        {
            await _durableClient.StartNewAsync(
                nameof(DelayedStoppedApprenticeshipHandler.DelayApprenticeshipStopped),
                $"StoppedApprenticeshipFor{message.ApprenticeshipId}",
                message);
        }

        public async Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            var instanceId = $"StoppedApprenticeshipFor{message.ContinuationOfId}";

            var state = await _durableClient.GetStatusAsync(instanceId);

            if (state?.RuntimeStatus == OrchestrationRuntimeStatus.Running)
                await _durableClient.RaiseEventAsync(instanceId, "ApprenticeshipContinued");
        }
    }

    public class DelayedStoppedApprenticeshipHandler
    {
        private readonly IEcsApi _api;

        public DelayedStoppedApprenticeshipHandler(IEcsApi api) => _api = api;

        [FunctionName(nameof(DelayApprenticeshipStopped))]
        public static async Task DelayApprenticeshipStopped(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            //var deadline = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(14));
            try
            {
                var deadline = TimeSpan.FromMinutes(1);
                await context.WaitForExternalEvent("ApprenticeshipContinued", deadline);
            }
            catch(TimeoutException)
            {
                var message = context.GetInput<ApprenticeshipStoppedEvent>();
                await context.CallActivityAsync(nameof(SendApprenticeStopped), message);
            }
        }

        [FunctionName(nameof(SendApprenticeStopped))]
        public void SendApprenticeStopped(
            [ActivityTrigger] ApprenticeshipStoppedEvent message,
            ILogger log)
        {
            log.LogInformation("Notifying API of stopped apprenticeship {commitmentsApprenticeshipId}", message.ApprenticeshipId);
            _api.StopApprenticeship(new ApprenticeshipStopped
            {
                CommitmentsApprenticeshipId = message.ApprenticeshipId,
                CommitmentsStoppedOn = message.StopDate,
            });
        }
    }
}