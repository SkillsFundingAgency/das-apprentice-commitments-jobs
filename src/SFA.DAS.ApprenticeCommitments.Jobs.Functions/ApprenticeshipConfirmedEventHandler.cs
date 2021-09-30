using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipConfirmedEventHandler : IHandleMessages<ApprenticeshipConfirmationConfirmedEvent>
    {
        private readonly IEcsApi _api;
        private readonly EmailService _emailer;
        private readonly ILogger<ApprenticeshipCommitmentsJobsHandler> _logger;

        public ApprenticeshipConfirmedEventHandler(
            IEcsApi api,
            EmailService emailer,
            ILogger<ApprenticeshipCommitmentsJobsHandler> logger)
        {
            _api = api;
            _emailer = emailer;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipConfirmationConfirmedEvent request, IMessageHandlerContext context)
        {
            _logger.LogInformation("Handle ApprenticeshipConfirmationConfirmedEvent for apprenticeship registration {ApprenticeshipId}", request.ApprenticeshipId);

            var apprentice = await _api.GetApprentice(request.ApprenticeId);

            await _emailer.SendApprenticeshipConfirmed(context, apprentice.Email, apprentice.FirstName);
        }
    }
}
