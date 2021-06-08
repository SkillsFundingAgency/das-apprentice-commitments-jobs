using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipCreatedSimulationHttpTrigger
    {
        private readonly IFunctionEndpoint endpoint;

        public ApprenticeshipCreatedSimulationHttpTrigger(IFunctionEndpoint endpoint) => this.endpoint = endpoint;

        [FunctionName("HandleApprenticeshipCreatedEventTrigger")]
        public Task<IActionResult> ApprenticeshipCreatedEvent(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "test-apprenticeship-created-event")] HttpRequestMessage req,
            ExecutionContext executionContext,
            ILogger log)
            => Simulate<ApprenticeshipCreatedEvent>(req, executionContext, log);

        [FunctionName("HandleApprenticeshipUpdatedEventTrigger")]
        public Task<IActionResult> ApprenticeshipUpdatedEvent(
            [HttpTrigger] HttpRequestMessage req, ExecutionContext executionContext, ILogger log)
            => Simulate<ApprenticeshipUpdatedApprovedEvent>(req, executionContext, log);

        public async Task<IActionResult> Simulate<T>(HttpRequestMessage req, ExecutionContext executionContext, ILogger log)
        {
            try
            {
                var @event = JsonConvert.DeserializeObject<T>(await req.Content.ReadAsStringAsync());

                await endpoint.Publish(@event, executionContext, log);

                return new AcceptedResult();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }
    }
}