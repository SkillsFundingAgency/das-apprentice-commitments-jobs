using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Handlers.CommitmentsEventHandlers
{
    public class CommitmentsEventHandler
        : IHandleMessages<ApprenticeshipCreatedEvent>
        , IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
    {
        private readonly IEcsApi _api;
        private readonly ILogger<CommitmentsEventHandler> _logger;

        public CommitmentsEventHandler(
            IEcsApi api,
            ILogger<CommitmentsEventHandler> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            if (!ShouldProcessTrainingType(message.TrainingType))
            {
                _logger.LogInformation(
                    "Ignoring ApprenticeshipCreatedEvent for {ApprenticeshipId} due to TrainingType={TrainingType}",
                    message.ApprenticeshipId,
                    message.TrainingType);

                return;
            }

            _logger.LogInformation(
                "Handling ApprenticeshipCreatedEvent for {ApprenticeshipId} (continuation {ContinuationOfId})",
                message.ApprenticeshipId,
                message.ContinuationOfId);

            if (message.ContinuationOfId.HasValue)
                await _api.UpdateApproval(message.ToApprenticeshipUpdated());
            else
                await _api.CreateApproval(message.ToApprenticeshipCreated());
        }

        public Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
        {
            if (!ShouldProcessTrainingType(message.TrainingType))
            {
                _logger.LogInformation(
                    "Ignoring ApprenticeshipUpdatedApprovedEvent for {ApprenticeshipId} due to TrainingType={TrainingType}",
                    message.ApprenticeshipId,
                    message.TrainingType);

                return Task.CompletedTask;
            }

            _logger.LogInformation(
                "Handling ApprenticeshipUpdatedApprovedEvent for {ApprenticeshipId}",
                message.ApprenticeshipId);

            return _api.UpdateApproval(message.ToApprenticeshipUpdated());
        }

        private static bool ShouldProcessTrainingType(ProgrammeType trainingType)
        {
            return trainingType == ProgrammeType.Standard;
        }
    }
}
