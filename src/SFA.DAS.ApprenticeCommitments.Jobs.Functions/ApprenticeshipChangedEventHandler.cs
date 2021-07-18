using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipChangedEventHandler : IHandleMessages<ApprenticeshipChangedEvent>
    {
        private TimeSpan _timeToWaitBeforeEmail => settings.TimeToWaitBeforeChangeOfApprenticeshipEmail;
        private readonly ApplicationSettings settings;
        private readonly IEcsApi api;
        private readonly ILogger<ApprenticeshipChangedEventHandler> logger;

        public ApprenticeshipChangedEventHandler(ApplicationSettings settings, IEcsApi api,ILogger<ApprenticeshipChangedEventHandler> logger)
        {
            this.api = api;
            this.settings = settings;
            this.logger = logger;
        }

        public async Task Handle(ApprenticeshipChangedEvent message, IMessageHandlerContext context)
        {
            if (!settings.Notifications.Templates.TryGetValue("ApprenticeshipChangedEmail", out var templateId))
                throw new InvalidOperationException("Missing configuration `Notifications:Templates:ApprenticeshipChangedEmail`");

            var url = $"{settings.ApprenticeCommitmentsWeb.BaseUrl}/Apprenticeships";

            var (apprentice, apprenticeship) = await GetApprenticeship(message);

            var ordered = apprenticeship.Revisions
                .OrderBy(x => x.ApprovedOn).ToArray();

            logger.LogInformation($"Revisions: {JsonConvert.SerializeObject(ordered)}");

            var newest = ordered[^1];
            var previous = ordered[^2];

            var sinceLastApproval = newest.ApprovedOn - previous.ApprovedOn;
            var seenPreviousApproval = apprenticeship.LastViewed > previous.ApprovedOn;

            if (sinceLastApproval > _timeToWaitBeforeEmail || seenPreviousApproval)
            {
                await context.Send(new SendEmailCommand(
                    templateId,
                    apprentice.Email,
                    new Dictionary<string, string>
                    {
                        { "GivenName", apprentice.FirstName },
                        { "FamilyName", apprentice.LastName },
                        { "ConfirmApprenticeshipUrl", url },
                    }));
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