using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public class ApprenticeshipUpdated
    {
        public long? CommitmentsContinuationApprenticeshipId { get; set; }
        public long CommitmentsApprenticeshipId { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }
    }
}