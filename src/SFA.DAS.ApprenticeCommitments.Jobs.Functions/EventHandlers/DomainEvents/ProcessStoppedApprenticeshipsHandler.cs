using Microsoft.Extensions.Logging;
using NServiceBus;
using RestEase;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.CommitmentsEventHandlers;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.InternalMessages.Commands;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.DomainEvents
{
    public class ProcessStoppedApprenticeshipsHandler : IHandleMessages<ProcessStoppedApprenticeship>
    {
        private readonly ILogger<StoppedApprenticeshipHandler> _logger;
        private readonly IEcsApi _api;

        public ProcessStoppedApprenticeshipsHandler(IEcsApi api, ILogger<StoppedApprenticeshipHandler> logger)
            => (_api, _logger) = (api, logger);

        public async Task Handle(ProcessStoppedApprenticeship message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation("Notifying API of stopped apprenticeship {commitmentsApprenticeshipId}", message.CommitmentsApprenticeshipId);
                await _api.StopApprenticeship(new ApprovalStopped
                {
                    CommitmentsApprenticeshipId = message.CommitmentsApprenticeshipId,
                    CommitmentsStoppedOn = message.CommitmentsStoppedOn,
                });
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Stopped apprenticeship {commitmentsApprenticeshipId} does not exist in domain", message.CommitmentsApprenticeshipId);
            }
        }
    }
}