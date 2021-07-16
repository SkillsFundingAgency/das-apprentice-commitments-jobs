using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Apprentice.LoginService.Messages;
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
        private readonly IFunctionEndpoint _endpoint;
        private readonly ApplicationSettings _options;

        public SendInvitationRemindersHandler(
            IEcsApi api,
            IFunctionEndpoint endpoint,
            ApplicationSettings options)
        {
            _api = api;
            _endpoint = endpoint;
            _options = options;
        }

        [FunctionName("HandleSendInvitationReminders")]
        public async Task Run(
            [TimerTrigger("%SendRemindersTriggerTime%")] TimerInfo _,
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
                    log.LogInformation($"Sending Invitation for Apprentice {registration.ApprenticeId}");
                    var invite = new SendInvitation
                    {
                        ClientId = _options.ApprenticeLoginApi.IdentityServerClientId,
                        SourceId = registration.ApprenticeId.ToString(),
                        Email = registration.Email,
                        GivenName = registration.FirstName,
                        FamilyName = registration.LastName,
                        OrganisationName = registration.EmployerName,
                        ApprenticeshipName = registration.CourseName,
                        Callback = new Uri(_options.ApprenticeLoginApi.CallbackUrl),
                        UserRedirect = new Uri(_options.ApprenticeLoginApi.RedirectUrl),
                    };

                    await _endpoint.Send(invite, executionContext, log);

                    log.LogInformation($"Updating Registration for Apprentice {registration.ApprenticeId}");
                    await _api.InvitationReminderSent(registration.ApprenticeId, new RegistrationReminderSentRequest
                    {
                        SentOn = DateTime.UtcNow
                    });
                }
                catch (Exception e)
                {
                    log.LogError(e, $"Error Sending a Reminder for Apprentice {registration.ApprenticeId}");
                }
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