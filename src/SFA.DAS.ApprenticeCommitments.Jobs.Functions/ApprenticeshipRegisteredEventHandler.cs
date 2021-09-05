using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipRegisteredEventHandler : IHandleMessages<ApprenticeshipRegisteredEvent>
    {
        private readonly IEcsApi _api;
        private readonly EmailService _emailer;
        private readonly ApplicationSettings _settings;
        private readonly ILogger<ApprenticeshipCommitmentsJobsHandler> _logger;

        public ApprenticeshipRegisteredEventHandler(
            IEcsApi api,
            EmailService emailer,
            ApplicationSettings settings,
            ILogger<ApprenticeshipCommitmentsJobsHandler> logger)
        {
            _api = api;
            _emailer = emailer;
            _settings = settings;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipRegisteredEvent request, IMessageHandlerContext context)
        {
            _logger.LogInformation("Apprenticeship registration {RegistrationId}", request.RegistrationId);

            var link = $"{_settings.ApprenticeLoginApi.RedirectUrl}?Register={request.RegistrationId}";
            
            var registration = await GetRegistration(request.RegistrationId);

            _logger.LogInformation($"Link `{{LoginLink}}` for apprenticeship registration {{registration}}", link, JsonConvert.SerializeObject(registration));

            await _emailer.SendApprenticeSignUpInvitation(context, registration.Email, registration.FirstName, link);
        }

        private async Task<Registration> GetRegistration(System.Guid registrationId)
        {
            var registration = await _api.GetRegistration(registrationId);
            return registration;
        }
    }
}
