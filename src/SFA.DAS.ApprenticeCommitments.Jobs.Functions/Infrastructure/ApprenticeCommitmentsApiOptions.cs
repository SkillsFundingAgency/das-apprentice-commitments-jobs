using SFA.DAS.Http.Configuration;
using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure
{
    public class ApprenticeCommitmentsApiOptions : IApimClientConfiguration
    {
        public const string ApprenticeCommitmentsApi = "ApprenticeCommitmentsApi";
        public string ApiBaseUrl { get; set; } = null!;
        public string SubscriptionKey { get; set; } = null!;
        public string ApiVersion { get; set; } = null!;
    }

    public class NServiceBusOptions

    {
        public Guid IdentityServerClientId { get; set; }

        public string CallbackUrl { get; set; } = null!;

        public string RedirectUrl { get; set; } = null!;
    }
}