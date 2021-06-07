using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public static class Mappings
    {
        public static ApprenticeshipCreated ToApprenticeshipCreated(this ApprenticeshipCreatedEvent source) =>
            new ApprenticeshipCreated
            {
                CommitmentsApprenticeshipId = source.ApprenticeshipId,
                EmployerName = source.LegalEntityName,
                EmployerAccountId = source.AccountId,
                EmployerAccountLegalEntityId = source.AccountLegalEntityId,
                TrainingProviderId = source.ProviderId,
                CommitmentsApprovedOn = source.CreatedOn
            };

        public static ApprenticeshipUpdated ToApprenticeshipUpdated(this ApprenticeshipCreatedEvent source) =>
            new ApprenticeshipUpdated
            {
                CommitmentsContinuedApprenticeshipId = source.ContinuationOfId,
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