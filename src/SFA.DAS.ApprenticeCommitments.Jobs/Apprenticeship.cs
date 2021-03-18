namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public class Apprenticeship
    {
        public long ApprenticeshipId { get; set; }
        public long EmployerAccountId { get; set; }
        public string Email { get; set; }
        public long EmployerAccountLegalEntityId { get; set; }
        public string EmployerName { get; set; }
        public long TrainingProviderId { get; set; }
    }
}