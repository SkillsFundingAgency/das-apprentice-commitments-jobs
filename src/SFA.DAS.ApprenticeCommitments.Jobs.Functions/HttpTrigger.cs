using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class HttpTrigger
    {
        private readonly ApprenticeshipCreatedHandler handler;

        public HttpTrigger(ApprenticeshipCreatedHandler handler) => this.handler = handler;

        [FunctionName("HandleApprenticeshipCreatedEventTrigger")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "test-apprenticeship-created-event")] HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation("Calling test-event.");

            try
            {
                var @event = JsonConvert.DeserializeObject<ApprenticeshipCreated2Event>(await req.Content.ReadAsStringAsync());
                await handler.Handle(@event, null);

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