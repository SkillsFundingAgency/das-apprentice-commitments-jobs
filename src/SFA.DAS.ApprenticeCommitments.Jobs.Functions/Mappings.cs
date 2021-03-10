using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public static class Mappings
    {
        public static Apprenticeship ToApprenticeship(this ApprenticeshipCreated2Event source) =>
            new Apprenticeship
            {
                ApprenticeshipId = source.ApprenticeshipId,
                Email = source.Email,
                EmployerName = source.LegalEntityName,
                EmployerAccountId = source.AccountId,
                AccountLegalEntityId = source.AccountLegalEntityId
            };
    }
}