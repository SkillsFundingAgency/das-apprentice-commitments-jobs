using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Api
{
    public class ApprenticeshipCreated
    {
        public long CommitmentsApprenticeshipId { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }
        public long EmployerAccountId { get; set; }
        public string Email { get; set; } = null!;
        public long EmployerAccountLegalEntityId { get; set; }
        public string EmployerName { get; set; } = null!;
        public long TrainingProviderId { get; set; }
    }
}