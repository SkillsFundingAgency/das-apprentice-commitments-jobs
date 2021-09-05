using Microsoft.Extensions.Logging;
using NServiceBus;
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
            string emailAddress,
            string firstName,
            string link)
        {
            await SendEmail(context, emailAddress,
                _settings.Notifications.ApprenticeSignUpInvitation,
                new Dictionary<string, string>
                {
                    { "GivenName", firstName },
                    { "CreateAccountLink", link },
                    { "LoginLink", link },
                });
        }

        internal async Task SendApprenticeshipChanged(IMessageHandlerContext context, string emailAddress, string firstName, string lastName, string url)
        {
            await SendEmail(context, emailAddress,
                _settings.Notifications.ApprenticeshipChangedEmail,
                new Dictionary<string, string>
                {
                    { "GivenName", firstName },
                    { "FamilyName", lastName },
                    { "ConfirmApprenticeshipUrl", url },
                });
        }

        private async Task SendEmail(IMessageHandlerContext context, string emailAddress, Guid templateId, Dictionary<string, string> tokens)
        {
            var message = new SendEmailCommand(templateId.ToString(), emailAddress, tokens);
            _logger.LogInformation($"Send {{emailTemplateId}} to {{email}} with {{tokens}}", templateId, emailAddress, tokens);
            await context.Send(message);
        }
    }
}