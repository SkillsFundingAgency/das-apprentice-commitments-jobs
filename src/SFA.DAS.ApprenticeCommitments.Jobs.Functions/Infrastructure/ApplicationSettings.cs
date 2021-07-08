using System;
using System.Collections.Generic;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure
{
    public class ApplicationSettings
    {
        public ApprenticeCommitmentsApiOptions ApprenticeCommitmentsApi { get; set; } = null!;
        public UrlConfiguration ApprenticeCommitmentsWeb { get; set; } = null!;
        public NotificationConfiguration Notifications { get; set; } = null!;
        public TimeSpan TimeToWaitBeforeChangeOfApprenticeshipEmail { get; set; } = TimeSpan.FromHours(24);
    }

    public class UrlConfiguration
    {
        public string BaseUrl { get; set; } = null!;
    }

    public class NotificationConfiguration
    {
        public Dictionary<string, string> Templates { get; set; } = new Dictionary<string, string>();
    }
}