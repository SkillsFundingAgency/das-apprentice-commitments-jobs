using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Handlers.DomainEvents
{
    public class ApprenticeshipRegisteredEventHandler : IHandleMessages<ApprenticeshipRegisteredEvent>
    {
        private readonly IEcsApi _api;
        private readonly EmailService _emailer;
        private readonly ILogger<ApprenticeshipConfirmedEventHandler> _logger;

        public ApprenticeshipRegisteredEventHandler(
            IEcsApi api,
            EmailService emailer,
            UrlConfiguration urls,
            ILogger<ApprenticeshipConfirmedEventHandler> logger)
        {
            _api = api;
            _emailer = emailer;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipRegisteredEvent request, IMessageHandlerContext context)
        {
            _logger.LogInformation("Handle ApprenticeshipRegisteredEvent for apprenticeship registration {RegistrationId}", request.RegistrationId);

            var registration = await GetRegistration(request.RegistrationId);

            await _emailer.SendApprenticeSignUpInvitation(context, request.RegistrationId, registration.Email, registration.FirstName);
        }

        private async Task<Registration> GetRegistration(System.Guid registrationId)
        {
            var registration = await _api.GetRegistration(registrationId);
            return registration;
        }
    }
}
