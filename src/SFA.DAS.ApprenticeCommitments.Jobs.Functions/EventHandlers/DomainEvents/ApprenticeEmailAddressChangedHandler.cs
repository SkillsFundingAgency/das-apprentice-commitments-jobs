using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeAccounts.Messages.Events;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.DomainEvents
{
    public class ApprenticeEmailAddressChangedHandler : IHandleMessages<ApprenticeEmailAddressChanged>
    {
        private readonly IEcsApi _api;
        private readonly ILogger<ApprenticeshipChangedEventHandler> _logger;

        public ApprenticeEmailAddressChangedHandler(IEcsApi api, ILogger<ApprenticeshipChangedEventHandler> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task Handle(ApprenticeEmailAddressChanged message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Apprentice {ApprenticeId} changed their email address", message.ApprenticeId);
            var apprenticeships = await _api.GetApprenticeships(message.ApprenticeId);

            foreach (var a in apprenticeships.Apprenticeships)
            {
                await context.Publish(new ApprenticeshipEmailAddressChangedEvent
                {
                    ApprenticeId = a.ApprenticeId,
                    CommitmentsApprenticeshipId = a.CommitmentsApprenticeshipId,
                });
            }
        }
    }
}