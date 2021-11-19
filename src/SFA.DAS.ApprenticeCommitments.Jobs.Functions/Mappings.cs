using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public static class Mappings
    {
        public static ApprovalCreated ToApprenticeshipCreated(this ApprenticeshipCreatedEvent source) =>
            new ApprovalCreated
            {
                CommitmentsApprenticeshipId = source.ApprenticeshipId,
                EmployerName = source.LegalEntityName,
                EmployerAccountId = source.AccountId,
                EmployerAccountLegalEntityId = source.AccountLegalEntityId,
                TrainingProviderId = source.ProviderId,
                CommitmentsApprovedOn = source.CreatedOn
            };

        public static ApprovalUpdated ToApprenticeshipUpdated(this ApprenticeshipCreatedEvent source) =>
            new ApprovalUpdated
            {
                CommitmentsContinuedApprenticeshipId = source.ContinuationOfId,
                CommitmentsApprenticeshipId = source.ApprenticeshipId,
                CommitmentsApprovedOn = source.CreatedOn
            };

        public static ApprovalUpdated ToApprenticeshipUpdated(this ApprenticeshipUpdatedApprovedEvent source) =>
            new ApprovalUpdated
            {
                CommitmentsApprenticeshipId = source.ApprenticeshipId,
                CommitmentsApprovedOn = source.ApprovedOn
            };

        public static ApprovalUpdated ToApprenticeshipUpdated(this ApprenticeshipUpdatedEmailAddressEvent source) =>
            new ApprovalUpdated
            {
                CommitmentsApprenticeshipId = source.ApprenticeshipId,
                CommitmentsApprovedOn = source.ApprovedOn
            };
    }
}