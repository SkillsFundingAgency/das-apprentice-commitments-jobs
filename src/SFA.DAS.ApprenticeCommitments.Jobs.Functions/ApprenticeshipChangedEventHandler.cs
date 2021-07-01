using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.Notifications.Messages.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipChangedEventHandler : IHandleMessages<ApprenticeshipChangedEvent>
    {
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
                throw new System.Exception("Missing configuration `Notifications:Templates:ApprenticeshipChangedEmail`");

            var url = $"{settings.ApprenticeCommitmentsWeb.BaseUrl}/Apprenticeships";

            var apprentice = await api.GetApprentice(message.ApprenticeId);

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
}