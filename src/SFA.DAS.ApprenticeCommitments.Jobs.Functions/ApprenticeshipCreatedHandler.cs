using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
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
            await api.CreateApprentice(apprenticeshipCreated.ToApprenticeship());
        }

        [FunctionName("HandleApprenticeshipCreatedEventTrigger")]
        public async Task<IActionResult> Run(
            [HttpTrigger("get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            await RunEvent(new ApprenticeshipCreatedEvent2
            {
                Email = "email@example.com",
            });

            return new OkResult();
        }
    }
}