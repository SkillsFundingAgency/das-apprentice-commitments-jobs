using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Apprentice.LoginService.Messages;
using SFA.DAS.Apprentice.LoginService.Messages.Commands;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipCommitmentsJobsHandler
        : IHandleMessages<ApprenticeshipCreatedEvent>
        , IHandleMessages<SendInvitationReply>
        , IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
        , IHandleMessages<UpdateEmailAddressCommand>
    {
        private readonly IEcsApi _api;
        private readonly ILogger<ApprenticeshipCommitmentsJobsHandler> _logger;

        public ApprenticeshipCommitmentsJobsHandler(
            IEcsApi api,
            ILogger<ApprenticeshipCommitmentsJobsHandler> logger)
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

        public Task Handle(UpdateEmailAddressCommand message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received {nameof(UpdateEmailAddressCommand)} for apprentice {message.ApprenticeId}");
            var requestBody = new JsonPatchDocument<Api.Apprentice>().Replace(x => x.Email, message.NewEmailAddress);
            return _api.UpdateApprentice(message.ApprenticeId, requestBody);
        }

        public async Task Handle(SendInvitationReply message, IMessageHandlerContext context)
            => await Task.CompletedTask;
    }
}