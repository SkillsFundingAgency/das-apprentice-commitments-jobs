using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Apprentice.LoginService.Messages;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.CommitmentsEventHandlers
{
    public class CommitmentsEventHandler
        : IHandleMessages<ApprenticeshipCreatedEvent>
        , IHandleMessages<SendInvitationReply>
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
            _logger.LogInformation("Handling ApprenticeshipCreatedEvent for {ApprenticeshipId} (continuation {ContinuationOfId})"
                , message.ApprenticeshipId, message.ContinuationOfId);

            if (message.ContinuationOfId.HasValue)
                await _api.UpdateApprenticeship(message.ToApprenticeshipUpdated());
            else
                await _api.CreateApprentice(message.ToApprenticeshipCreated());
        }

        public Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Handling ApprenticeshipUpdatedApprovedEvent for {ApprenticeshipId}", message.ApprenticeshipId);
            return _api.UpdateApprenticeship(message.ToApprenticeshipUpdated());
        }

        public async Task Handle(SendInvitationReply message, IMessageHandlerContext context)
            => await Task.CompletedTask;
    }
}