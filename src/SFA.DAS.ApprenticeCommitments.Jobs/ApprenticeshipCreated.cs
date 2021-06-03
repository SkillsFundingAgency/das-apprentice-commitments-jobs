using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public class ApprenticeshipCreated
    {
        public long CommitmentsApprenticeshipId { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }
        public long EmployerAccountId { get; set; }
        public string Email { get; set; }
        public long EmployerAccountLegalEntityId { get; set; }
        public string EmployerName { get; set; }
        public long TrainingProviderId { get; set; }
    }
}