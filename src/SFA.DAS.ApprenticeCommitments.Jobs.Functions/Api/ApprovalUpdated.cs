using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public class ApprovalUpdated
    {
        public long? CommitmentsContinuedApprenticeshipId { get; set; }
        public long CommitmentsApprenticeshipId { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }
    }
}