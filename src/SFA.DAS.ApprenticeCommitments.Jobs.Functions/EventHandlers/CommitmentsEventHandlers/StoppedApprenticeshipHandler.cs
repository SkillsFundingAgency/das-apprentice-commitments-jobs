using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
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
        private readonly ApplicationSettings _settings;

        public DelayedStoppedApprenticeshipHandler(IEcsApi api, ApplicationSettings settings)
            => (_api, _settings) = (api, settings);

        [FunctionName(nameof(DelayApprenticeshipStopped))]
        public async Task DelayApprenticeshipStopped(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger)
        {
            try
            {
                var deadline = _settings.TimeToWaitBeforeStoppingApprenticeship;
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