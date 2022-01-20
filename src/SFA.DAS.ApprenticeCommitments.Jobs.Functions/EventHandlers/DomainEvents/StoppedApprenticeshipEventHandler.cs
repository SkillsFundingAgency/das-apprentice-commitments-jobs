using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.DomainEvents
{
    public class StoppedApprenticeshipEventHandler : IHandleMessages<ApprenticeshipStoppedEvent>
    {
        private readonly IEcsApi _api;
        private readonly EmailService _emailer;
        private readonly ILogger<StoppedApprenticeshipEventHandler> _logger;

        public StoppedApprenticeshipEventHandler(
            IEcsApi api,
            EmailService emailer,
            ILogger<StoppedApprenticeshipEventHandler> logger)
        {
            _api = api;
            _emailer = emailer;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Handle ApprenticeshipStoppedEvent for apprenticeship {RegistrationId}", message.ApprenticeshipId);

            var (email, firstName) = await GetApprenticeDetails(message);

            await _emailer.SendApprenticeshipStopped(context,
                email,
                firstName,
                message.EmployerName,
                message.CourseName);
        }

        private async Task<(string email, string firstName)> GetApprenticeDetails(ApprenticeshipStoppedEvent message)
        {
            if (message.ApprenticeId != null)
            {
                var apprentice = await _api.GetApprentice(message.ApprenticeId.Value);
                return (apprentice.Email, apprentice.FirstName);
            }
            else
            {
                var registration = await _api.GetApprovalsRegistration(message.CommitmentsApprenticeshipId);
                return (registration.Email, registration.FirstName);
            }
        }
    }
}