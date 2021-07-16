using System;
using System.Collections.Generic;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public class ApprenticeshipHistory
    {
        public long ApprenticeshipId { get; set; }
        public DateTime LastViewed { get; set; }
        public List<ApprenticeshipRevision> Revisions { get; set; } = new List<ApprenticeshipRevision>();
    }
}