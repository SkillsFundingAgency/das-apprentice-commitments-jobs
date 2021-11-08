using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.DomainEvents
{
    public class ApprenticeshipChangedEventHandler : IHandleMessages<ApprenticeshipChangedEvent>
    {
        private TimeSpan TimeToWaitBeforeEmail => settings.TimeToWaitBeforeChangeOfApprenticeshipEmail;
        private readonly ApplicationSettings settings;
        private readonly IEcsApi api;
        private readonly EmailService emailer;
        private readonly ILogger<ApprenticeshipChangedEventHandler> logger;

        public ApprenticeshipChangedEventHandler(
            IEcsApi api,
            EmailService emailer,
            ApplicationSettings settings,
            ILogger<ApprenticeshipChangedEventHandler> logger)
        {
            this.api = api;
            this.emailer = emailer;
            this.settings = settings;
            this.logger = logger;
        }

        public async Task Handle(ApprenticeshipChangedEvent message, IMessageHandlerContext context)
        {
            var (apprentice, apprenticeship) = await GetApprenticeship(message);

            var ordered = apprenticeship.Revisions
                .OrderBy(x => x.ApprovedOn).ToArray();

            logger.LogInformation($"Revisions: {JsonConvert.SerializeObject(ordered)}");

            var newest = ordered[^1];
            var previous = ordered[^2];

            var sinceLastApproval = newest.ApprovedOn - previous.ApprovedOn;
            var seenPreviousApproval = apprenticeship.LastViewed > previous.ApprovedOn;

            if (sinceLastApproval > TimeToWaitBeforeEmail || seenPreviousApproval)
            {
                await emailer.SendApprenticeshipChanged(context,
                    apprentice.Email, apprentice.FirstName, apprentice.LastName);
            }
        }

        private async Task<(Api.Apprentice, ApprenticeshipHistory)> GetApprenticeship(ApprenticeshipChangedEvent message)
        {
            var apprentice = api.GetApprentice(message.ApprenticeId);
            var apprenticeship = api.GetApprenticeshipHistory(message.ApprenticeId, message.ApprenticeshipId);
            await Task.WhenAll(apprentice, apprenticeship);
            return (apprentice.Result, apprenticeship.Result);
        }
    }
}