using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public class ApprenticeshipUpdated
    {
        public long ApprenticeshipId { get; set; }
        public DateTime ApprovedOn { get; set; }
        public string Email { get; set; }
    }
}