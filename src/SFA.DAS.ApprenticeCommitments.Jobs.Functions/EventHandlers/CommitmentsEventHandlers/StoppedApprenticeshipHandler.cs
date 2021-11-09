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
        private readonly ILogger<StoppedApprenticeshipHandler> _logger;

        public StoppedApprenticeshipHandler(
            IDurableClientFactory clientFactory,
            IConfiguration configuration,
            ILogger<StoppedApprenticeshipHandler> logger)
        {
            _durableClient = clientFactory.CreateClient(new DurableClientOptions
            {
                TaskHub = configuration["TaskHub"],
            });
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Starting delay for stopped apprenticeship {apprenticeshipId}", message.ApprenticeshipId);
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
            {
                _logger.LogInformation("Interrupting stopped apprenticeship {apprenticeshipId} - continued by {continuationOfId}", message.ApprenticeshipId, message.ContinuationOfId);
                await _durableClient.RaiseEventAsync(instanceId, "ApprenticeshipContinued");
            }
            else
            {
                _logger.LogInformation("Too late to interrupt stopped apprenticeship {apprenticeshipId} - continued by {continuationOfId}", message.ApprenticeshipId, message.ContinuationOfId);
            }
        }
    }

    public class DelayedStoppedApprenticeshipHandler
    {
        private readonly IEcsApi _api;

        public DelayedStoppedApprenticeshipHandler(IEcsApi api) => _api = api;

        [FunctionName(nameof(DelayApprenticeshipStopped))]
        public static async Task DelayApprenticeshipStopped(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger)
        {
            try
            {
                var deadline = TimeSpan.FromDays(14); // TODO needs to come from configuration
                await context.WaitForExternalEvent("ApprenticeshipContinued", deadline);
            }
            catch(TimeoutException)
            {
                var message = context.GetInput<ApprenticeshipStoppedEvent>();
                await context.CallActivityAsync(nameof(SendApprenticeStopped), message);
                logger.LogInformation("Notied API of stopped apprenticeship {apprenticeshipId}", message.ApprenticeshipId);
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