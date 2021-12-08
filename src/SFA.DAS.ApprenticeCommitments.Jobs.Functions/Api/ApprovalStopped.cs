using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public class ApprovalStopped
    {
        public long CommitmentsApprenticeshipId { get; set; }
        public DateTime CommitmentsStoppedOn { get; set; }
    }
}