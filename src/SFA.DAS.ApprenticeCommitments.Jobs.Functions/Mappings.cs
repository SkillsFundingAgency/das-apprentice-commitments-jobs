using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public static class Mappings
    {
        public static ApprenticeshipCreated ToApprenticeship(this ApprenticeshipCreated2Event source) =>
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

        public static ApprenticeshipUpdated ToApprenticeship(this ApprenticeshipUpdatedApproved2Event source) =>
            new ApprenticeshipUpdated
            {
                ApprenticeshipId = source.ApprenticeshipId,
                ApprovedOn = source.ApprovedOn,
                Email = source.Email,
            };
    }
}