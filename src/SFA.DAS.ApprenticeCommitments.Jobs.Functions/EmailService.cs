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
            await SendEmail(o => context.Send(o), emailAddress,
                _settings.Notifications.ApprenticeSignUp,
                new Dictionary<string, string>
                {
                    { "GivenName", firstName },
                    { "CreateAccountLink", link },
                    { "LoginLink", link },
                });
        }

        internal async Task SendApprenticeSignUpInvitation(
            Func<object, Task> send,
            string emailAddress,
            string firstName,
            string link)
        {
            await SendEmail(send, emailAddress,
                _settings.Notifications.ApprenticeSignUp,
                new Dictionary<string, string>
                {
                    { "GivenName", firstName },
                    { "CreateAccountLink", link },
                    { "LoginLink", link },
                });
        }

        internal async Task SendApprenticeshipChanged(IMessageHandlerContext context, string emailAddress, string firstName, string lastName, string url)
        {
            await SendEmail(o => context.Send(o), emailAddress,
                _settings.Notifications.ApprenticeshipChanged,
                new Dictionary<string, string>
                {
                    { "GivenName", firstName },
                    { "FamilyName", lastName },
                    { "ConfirmApprenticeshipUrl", url },
                });
        }
        
        private async Task SendEmail(Func<object, Task> send, string emailAddress, Guid templateId, Dictionary<string, string> tokens)
        {
            var message = new SendEmailCommand(templateId.ToString(), emailAddress, tokens);
            _logger.LogInformation($"Send {{emailTemplateId}} to {{email}} with {{tokens}}", templateId, emailAddress, tokens);
            await send(message);
        }
    }
}