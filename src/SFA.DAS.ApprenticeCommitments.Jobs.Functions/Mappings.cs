using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public static class Mappings
    {
        public static ApprenticeshipCreated ToApprenticeshipCreated(this ApprenticeshipCreated2Event source) =>
            new ApprenticeshipCreated
            {
                CommitmentsApprenticeshipId = source.ApprenticeshipId,
                Email = source.Email,
                EmployerName = source.LegalEntityName,
                EmployerAccountId = source.AccountId,
                EmployerAccountLegalEntityId = source.AccountLegalEntityId,
                TrainingProviderId = source.ProviderId,
                CommitmentsApprovedOn = source.CreatedOn
            };

        public static ApprenticeshipUpdated ToApprenticeshipUpdated(this ApprenticeshipCreated2Event source) =>
            new ApprenticeshipUpdated
            {
                CommitmentsContinuationApprenticeshipId = source.ContinuationOfId,
                CommitmentsApprenticeshipId = source.ApprenticeshipId,
                CommitmentsApprovedOn = source.CreatedOn
            };

        public static ApprenticeshipUpdated ToApprenticeshipUpdated(this ApprenticeshipUpdatedApprovedEvent source) =>
            new ApprenticeshipUpdated
            {
                CommitmentsApprenticeshipId = source.ApprenticeshipId,
                CommitmentsApprovedOn = source.ApprovedOn
            };
    }
}