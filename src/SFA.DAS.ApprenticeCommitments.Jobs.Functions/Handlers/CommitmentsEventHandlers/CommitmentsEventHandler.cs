using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Handlers.CommitmentsEventHandlers
{
    public class CommitmentsEventHandler
        : IHandleMessages<ApprenticeshipCreatedEvent>
        , IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
    {
        private readonly IEcsApi _api;
        private readonly ILogger<CommitmentsEventHandler> _logger;

        public CommitmentsEventHandler(IEcsApi api, ILogger<CommitmentsEventHandler> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            // Filter: ignore ApprenticeshipUnit
            if (message.LearningType == LearningType.ApprenticeshipUnit)
            {
                _logger.LogInformation(
                    "Ignoring ApprenticeshipCreatedEvent for {ApprenticeshipId} due to LearningType={LearningType}",
                    message.ApprenticeshipId, message.LearningType);
                return;
            }

            _logger.LogInformation(
                "Handling ApprenticeshipCreatedEvent for {ApprenticeshipId} (continuation {ContinuationOfId})",
                message.ApprenticeshipId, message.ContinuationOfId);

            if (message.ContinuationOfId.HasValue)
                await _api.UpdateApproval(message.ToApprenticeshipUpdated());
            else
                await _api.CreateApproval(message.ToApprenticeshipCreated());
        }

        public Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
        {
            // Filter: ignore ApprenticeshipUnit
            if (message.LearningType == LearningType.ApprenticeshipUnit)
            {
                _logger.LogInformation(
                    "Ignoring ApprenticeshipUpdatedApprovedEvent for {ApprenticeshipId} due to LearningType={LearningType}",
                    message.ApprenticeshipId, message.LearningType);
                return Task.CompletedTask;
            }

            _logger.LogInformation(
                "Handling ApprenticeshipUpdatedApprovedEvent for {ApprenticeshipId}",
                message.ApprenticeshipId);

            return _api.UpdateApproval(message.ToApprenticeshipUpdated());
        }
    }
}