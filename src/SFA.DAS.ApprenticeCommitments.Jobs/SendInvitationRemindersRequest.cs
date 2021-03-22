using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public class SendInvitationRemindersRequest
    {
        public DateTime SendNow { set; get; }
        public int RemindAfterDays { set; get; }
    }
}