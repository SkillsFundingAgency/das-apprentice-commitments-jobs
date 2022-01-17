using Microsoft.Extensions.Logging;
using NServiceBus;
using RestEase;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Messages.Commands;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.CommitmentsEventHandlers
{
    public class ApprenticeshipResendInvitationEventHandler
        : IHandleMessages<SendApprenticeshipInvitationCommand>
    {
        private readonly IEcsApi _api;
        private readonly EmailService _emailService;
        private readonly ILogger<ApprenticeshipResendInvitationEventHandler> _logger;

        public ApprenticeshipResendInvitationEventHandler(
            IEcsApi api,
            EmailService emailService,
            ILogger<ApprenticeshipResendInvitationEventHandler> logger)
        {
            _api = api;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(SendApprenticeshipInvitationCommand message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation("Handling ApprenticeshipResendInvitationEvent for {ApprenticeshipId}", message.CommitmentsApprenticeshipId);
                var registration = await _api.GetApprovalsRegistration(message.CommitmentsApprenticeshipId);
                if (!registration.ApprenticeId.HasValue)
                {
                    await _emailService.SendApprenticeSignUpInvitation(context, registration.RegistrationId, registration.Email, registration.FirstName);
                }
                else
                {
                    _logger.LogInformation("Commitments Apprenticeship {ApprenticeshipId}, already assigned to an apprentice, invitation email not required", message.CommitmentsApprenticeshipId);
                }
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Apprenticeship {commitmentsApprenticeshipId} does not exist in domain", message.CommitmentsApprenticeshipId);
            }
        }
    }
}