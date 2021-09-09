using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class EmailService
    {
        private readonly ILogger<ApprenticeshipCommitmentsJobsHandler> _logger;
        private readonly ApplicationSettings _settings;

        public EmailService(
            ApplicationSettings settings,
            ILogger<ApprenticeshipCommitmentsJobsHandler> logger)
        {
            _logger = logger;
            _settings = settings;
        }

        internal async Task SendApprenticeSignUpInvitation(
            IMessageHandlerContext context,
            Guid registrationId,
            string emailAddress,
            string firstName)
        {
            await SendApprenticeSignUpInvitation(
                o => context.Send(o),
                registrationId,
                emailAddress,
                firstName);
        }

        internal async Task SendApprenticeSignUpInvitation(
            Func<object, Task> send,
            Guid registrationId,
            string emailAddress,
            string firstName)
        {
            var link = $"{_settings.ApprenticeWeb.StartPageUrl}?Register={registrationId}";

            _logger.LogInformation($"Send ApprenticeSignUpInvitation ({{templateId}}) with {{link}}",
                _settings.Notifications.ApprenticeSignUp, link);

            await SendEmail(send, emailAddress,
                _settings.Notifications.ApprenticeSignUp,
                new Dictionary<string, string>
                {
                    { "GivenName", firstName },
                    { "CreateAccountLink", link },
                    { "LoginLink", link },
                });
        }

        internal async Task SendApprenticeshipChanged(
            IMessageHandlerContext context,
            string emailAddress,
            string firstName,
            string lastName)
        {
            await SendEmail(o => context.Send(o), emailAddress,
                _settings.Notifications.ApprenticeshipChanged,
                new Dictionary<string, string>
                {
                    { "GivenName", firstName },
                    { "FamilyName", lastName },
                    { "ConfirmApprenticeshipUrl", _settings.ApprenticeWeb.ConfirmApprenticeshipUrl.ToString() },
                });
        }
        
        private async Task SendEmail(Func<object, Task> send, string emailAddress, Guid templateId, Dictionary<string, string> tokens)
        {
            var message = new SendEmailCommand(templateId.ToString(), emailAddress, tokens);
            await send(message);
        }
    }
}