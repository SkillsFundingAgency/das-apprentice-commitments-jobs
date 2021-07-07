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

        public ApprenticeshipChangedEventHandler(ApplicationSettings settings, IEcsApi api)
        {
            this.api = api;
            this.settings = settings;
        }

        public async Task Handle(ApprenticeshipChangedEvent message, IMessageHandlerContext context)
        {
            if (!settings.Notifications.Templates.TryGetValue("ApprenticeshipChangedEmail", out var templateId))
                throw new Exception("Missing configuration `Notifications:Templates:ApprenticeshipChangedEmail`");

            var url = $"{settings.ApprenticeCommitmentsWeb.BaseUrl}/Apprenticeships";

            var (apprentice, apprenticeship) = await GetApprenticeship(message);

            var ordered = apprenticeship.Revisions
                .OrderBy(x => x.CommitmentsApprovedOn).ToArray();

            var newest = ordered[^1];
            var previous = ordered[^2];

            var sinceLastApproval = newest.CommitmentsApprovedOn - previous.CommitmentsApprovedOn;
            var seenPreviousApproval = apprenticeship.LastViewed > previous.CommitmentsApprovedOn;

            if (sinceLastApproval > _timeToWaitBeforeEmail || seenPreviousApproval)
            {
                await context.Send(new SendEmailCommand(
                    templateId,
                    apprentice.Email.ToString(),
                    new Dictionary<string, string>
                    {
                    { "GivenName", apprentice.FirstName },
                    { "FamilyName", apprentice.LastName },
                    { "ConfirmApprenticeshipUrl", url },
                    }));
            }
        }

        private async Task<(Apprentice, ApprenticeshipHistory)> GetApprenticeship(ApprenticeshipChangedEvent message)
        {
            var apprentice = api.GetApprentice(message.ApprenticeId);
            var apprenticeship = api.GetApprenticeshipHistory(message.ApprenticeId, message.ApprenticeshipId);
            await Task.WhenAll(apprentice, apprenticeship);
            return (apprentice.Result, apprenticeship.Result);
        }
    }
}