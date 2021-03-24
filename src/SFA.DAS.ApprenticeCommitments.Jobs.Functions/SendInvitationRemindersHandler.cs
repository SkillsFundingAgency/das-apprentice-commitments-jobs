using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class SendInvitationRemindersHandler
    {
        private readonly IEcsApi api;
        private readonly IConfiguration _configuration;

        private const int DefaultReminderAfterDays = 7;


        public SendInvitationRemindersHandler(IEcsApi api, IConfiguration configuration)
        {
            this.api = api;
            _configuration = configuration;
        }

        [FunctionName("HandleSendInvitationReminders")]
        public async Task Run([TimerTrigger("%SendRemindersTriggerTime%", RunOnStartup = true)]TimerInfo _, ILogger log)
        {
            try
            {
                log.LogInformation("Send Invitation Reminders Job Starting");
                await api.SendInvitationReminders(new SendInvitationRemindersRequest { InvitationCutOffTime = DateTime.UtcNow.AddDays(GetReminderAfterDaysValue())});
                log.LogInformation("Send Invitation Reminders Job Complete");
            }
            catch (Exception e)
            {
                log.LogError(e, "Send Invitation Reminders Job has failed");
            }
        }

        private int GetReminderAfterDaysValue()
        {
            var remindAfterDays = Convert.ToInt32(_configuration["SendRemindersAfterThisNumberDays"]);
            if (remindAfterDays <= 0)
            {
                return DefaultReminderAfterDays;
            }

            return remindAfterDays;
        }
    }
}