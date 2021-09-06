﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipRegisteredEventHandler : IHandleMessages<ApprenticeshipRegisteredEvent>
    {
        private readonly IEcsApi _api;
        private readonly EmailService _emailer;
        private readonly UrlConfiguration _urls;
        private readonly ILogger<ApprenticeshipCommitmentsJobsHandler> _logger;

        public ApprenticeshipRegisteredEventHandler(
            IEcsApi api,
            EmailService emailer,
            UrlConfiguration urls,
            ILogger<ApprenticeshipCommitmentsJobsHandler> logger)
        {
            _api = api;
            _emailer = emailer;
            _urls = urls;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipRegisteredEvent request, IMessageHandlerContext context)
        {
            _logger.LogInformation("Handle ApprenticeshipRegisteredEvent for apprenticeship registration {RegistrationId}", request.RegistrationId);

            var link = $"{_urls.StartPageUrl}?Register={request.RegistrationId}";
            
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
