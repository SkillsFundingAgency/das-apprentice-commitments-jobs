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
    public class ApprenticeshipCreatedHandler : IHandleMessages<ApprenticeshipCreated2Event>
    {
        private readonly IEcsApi api;

        public ApprenticeshipCreatedHandler(IEcsApi api) => this.api = api;

        public async Task Handle(ApprenticeshipCreated2Event message, IMessageHandlerContext _)
        {
            await api.CreateApprentice(message.ToApprenticeship());
        }
    }
}