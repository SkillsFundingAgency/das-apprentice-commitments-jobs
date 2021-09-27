using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class SendInvitationRemindersHandler
    {
        private readonly IEcsApi _api;
        private readonly EmailService _emailer;
        private readonly IFunctionEndpoint _endpoint;
        private readonly ApplicationSettings _options;

        public SendInvitationRemindersHandler(
            IEcsApi api,
            EmailService emailer,
            IFunctionEndpoint endpoint,
            ApplicationSettings options)
        {
            _api = api;
            _emailer = emailer;
            _endpoint = endpoint;
            _options = options;
        }

        [FunctionName("HandleSendInvitationReminders")]
        public async Task Run(
            [TimerTrigger("%SendRemindersTriggerTime%")] TimerInfo timer,
            ExecutionContext executionContext,
            ILogger log)
        {
            try
            {
                log.LogInformation("Getting Registrations which are due a reminder");

                var reminder = await _api.GetReminderRegistrations(GetReminderCutoff());

                await Task.WhenAll(reminder.Registrations.Select(SendReminder));

                log.LogInformation("Send Invitation Reminders Job Complete");
            }
            catch (Exception e)
            {
                log.LogError(e, "Send Invitation Reminders Job has failed");
            }

            DateTime GetReminderCutoff() => DateTime.UtcNow.AddDays(-ReminderAfterDays);

            async Task SendReminder(Registration registration)
            {
                try
                {
                    log.LogInformation($"Sending Invitation for Apprentice {registration.RegistrationId}");
                    
                    await _emailer.SendApprenticeSignUpInvitation(SendMessage,
                        registration.RegistrationId, registration.Email, registration.FirstName);

                    log.LogInformation($"Updating Registration for Apprentice {registration.RegistrationId}");
                    await _api.InvitationReminderSent(registration.RegistrationId, new RegistrationReminderSentRequest
                    {
                        SentOn = DateTime.UtcNow
                    });
                }
                catch (Exception e)
                {
                    log.LogError(e, $"Error Sending a Reminder for Apprentice {registration.RegistrationId}");
                }

                Task SendMessage(object message) => _endpoint.Send(message, executionContext, log);
            }
        }

        internal long ReminderAfterDays
        {
            get
            {
                var remindAfterDays = Convert.ToInt32(_options.SendRemindersAfterThisNumberDays);
                return remindAfterDays > 0 ? remindAfterDays : 7;
            }
        }
    }
}