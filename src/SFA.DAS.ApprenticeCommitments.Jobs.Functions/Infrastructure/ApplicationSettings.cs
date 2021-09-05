using SFA.DAS.Http.Configuration;
using System;
using System.Collections.Generic;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure
{
    public class ApplicationSettings
    {
        public ApprenticeCommitmentsApiOptions ApprenticeCommitmentsApi { get; set; } = null!;
        public UrlConfiguration ApprenticeCommitmentsWeb { get; set; } = null!;
        public NotificationConfiguration Notifications { get; set; } = null!;
        public long SendRemindersAfterThisNumberDays { get; set; }
        public TimeSpan TimeToWaitBeforeChangeOfApprenticeshipEmail { get; set; } = TimeSpan.FromHours(24);
    }

    public class UrlConfiguration
    {
        public Uri PortalBaseUrl { get; set; } = null!;
        public Uri StartPageUrl => PortalBaseUrl;
        public Uri ConfirmApprenticeshipUrl => new Uri(PortalBaseUrl, "Apprenticeships");
    }

    public class NotificationConfiguration
    {
        public Dictionary<string, string> Templates { get; set; } = new Dictionary<string, string>();

        public Guid ApprenticeSignUp => GetTemplateId("ApprenticeSignUp");
        public Guid ApprenticeshipChanged => GetTemplateId("ApprenticeshipChanged");

        private Guid GetTemplateId(string templateName)
        {
            return Templates.TryGetValue(templateName, out var templateId)
                ? Guid.Parse(templateId)
                : throw new MissingEmailTemplateConfigurationException(templateName);
        }
    }

    public class ApprenticeCommitmentsApiOptions : IApimClientConfiguration
    {
        public const string ApprenticeCommitmentsApi = "ApprenticeCommitmentsApi";
        public string ApiBaseUrl { get; set; } = null!;
        public string SubscriptionKey { get; set; } = null!;
        public string ApiVersion { get; set; } = null!;
    }
}