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
            _logger.LogInformation("Handle ApprenticeshipStoppedEvent for approval {CommitmentsApprenticeshipId}", message.CommitmentsApprenticeshipId);

            if (message.ApprenticeId != null)
            {
                var apprentice = await _api.GetApprentice(message.ApprenticeId.Value);
                await _emailer.SendApprenticeshipStopped(context,
                    apprentice.Email,
                    apprentice.FirstName,
                    message.EmployerName,
                    message.CourseName);
            }
            else if(message.RegistrationId != null)
            {
                var registration = await _api.GetRegistration(message.RegistrationId.Value);
                await _emailer.SendUnmatchedApprenticeshipStopped(context,
                    registration.Email,
                    registration.FirstName,
                    message.EmployerName,
                    message.CourseName,
                    message.RegistrationId.Value);
            }
            else
            {
                _logger.LogWarning(
                    "ApprenticeshipStoppedEvent for approval {CommitmentsApprenticeshipId} " +
                    "could not find Apprentice {ApprenticeId} or Registration {RegistrationId}",
                    message.CommitmentsApprenticeshipId, message.ApprenticeId, message.RegistrationId);
            }
        }
    }
}