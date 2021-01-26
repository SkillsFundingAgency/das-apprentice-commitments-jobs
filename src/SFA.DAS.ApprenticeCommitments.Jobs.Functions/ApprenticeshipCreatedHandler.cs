using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipCreated2Event : ApprenticeshipCreatedEvent
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
            [NServiceBusTrigger(Endpoint = QueueNames.ApprenticeshipCreatedEvent)] ApprenticeshipCreated2Event apprenticeshipCreated)
        {
            await api.CreateApprentice(apprenticeshipCreated.ToApprenticeship());
        }

        [FunctionName("HandleApprenticeshipCreatedEventTrigger")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "test-apprenticeship-created-event")] HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation("Calling test-event.");

            try
            {
                var @event = JsonConvert.DeserializeObject<ApprenticeshipCreated2Event>(await req.Content.ReadAsStringAsync());
                await RunEvent(@event);
                return new AcceptedResult();
            }
            catch (Exception e)
            {
                log.LogError(e, "Error Calling test-apprenticeship-created-event");
                return new BadRequestResult();
            }
        }
    }
}