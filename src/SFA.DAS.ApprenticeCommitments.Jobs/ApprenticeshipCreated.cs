using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public class ApprenticeshipCreated
    {
        public long ApprenticeshipId { get; set; }
        public DateTime AgreedOn { get; set; }
        public long EmployerAccountId { get; set; }
        public string Email { get; set; }
        public long EmployerAccountLegalEntityId { get; set; }
        public string EmployerName { get; set; }
        public long TrainingProviderId { get; set; }
    }
}