using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipCreated2Event : ApprenticeshipCreatedEvent
    {
        public string Email { get; set; }
    }
}

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipCommitmentsJobsHandler
        : IHandleMessages<ApprenticeshipCreated2Event>
        , IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
    {
        private readonly IEcsApi api;

        public ApprenticeshipCommitmentsJobsHandler(IEcsApi api) => this.api = api;

        public async Task Handle(ApprenticeshipCreated2Event message, IMessageHandlerContext _)
        {
            if (message.ContinuationOfId.HasValue)
            {
                await api.UpdateApprenticeship(message.ToApprenticeshipUpdated());
                return;
            }
            await api.CreateApprentice(message.ToApprenticeshipCreated());
        }

        public Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext _)
            => api.UpdateApprenticeship(message.ToApprenticeshipUpdated());
    }
}