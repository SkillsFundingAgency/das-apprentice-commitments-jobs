using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public static class Mappings
    {
        public static ApprenticeshipCreated ToApprenticeshipCreated(this ApprenticeshipCreated2Event source) =>
            new ApprenticeshipCreated
            {
                ApprenticeshipId = source.ApprenticeshipId,
                Email = source.Email,
                EmployerName = source.LegalEntityName,
                EmployerAccountId = source.AccountId,
                EmployerAccountLegalEntityId = source.AccountLegalEntityId,
                TrainingProviderId = source.ProviderId,
                AgreedOn = source.AgreedOn,
            };

        public static ApprenticeshipUpdated ToApprenticeshipUpdated(this ApprenticeshipCreated2Event source) =>
            new ApprenticeshipUpdated
            {
                ContinuationOfCommitmentsApprenticeshipId = source.ContinuationOfId,
                CommitmentsApprenticeshipId = source.ApprenticeshipId,
                CommitmentsApprovedOn = source.AgreedOn,
            };

        public static ApprenticeshipUpdated ToApprenticeshipUpdated(this ApprenticeshipUpdatedApprovedEvent source) =>
            new ApprenticeshipUpdated
            {
                CommitmentsApprenticeshipId = source.ApprenticeshipId,
                CommitmentsApprovedOn = source.ApprovedOn,
            };
    }
}