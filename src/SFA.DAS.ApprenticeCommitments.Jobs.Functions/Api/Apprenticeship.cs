using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public class Apprenticeship
    {
        public long Id { get; set; }

        public Guid ApprenticeId { get; set; }

        public long CommitmentsApprenticeshipId { get; set; }

        public DateTime ApprovedOn { get; set; }

        public DateTime? StoppedReceivedOn { get; set; }

        public bool IsStopped => StoppedReceivedOn != null;
    }
}