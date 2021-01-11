using Microsoft.Azure.WebJobs;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipCreatedEvent2 : ApprenticeshipCreatedEvent
    {
        public string Email { get; set; }
    }
}

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipCreatedHandler
    {
        private readonly IEcsApi api;

        public ApprenticeshipCreatedHandler(IEcsApi api) => this.api = api;

        [FunctionName("HandleApprenticeshipCreatedEvent")]
        public async Task RunEvent(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.EmployerIncentives.AddEmployerVendorId")] ApprenticeshipCreatedEvent apprenticeshipCreated)
        {
            if (apprenticeshipCreated is ApprenticeshipCreatedEvent2 evt2)
            {
                await api.CreateApprentice(evt2.Email);
            }
        }
    }
}