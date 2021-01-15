using FluentAssertions;
using NServiceBus.Transport;
using SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Steps
{
    [Binding]
    public class NewApprenticeshipSteps : StepsBase
    {
        public NewApprenticeshipSteps(TestContext testContext) : base(testContext)
        {
        }

        [When("an apprenticeship is approved")]
        public async Task WhenAnApprenticeshipIsApproved()
        {
            testContext.Api.AcceptAllRequests();

            await testContext.WaitFor<MessageContext>(async () =>
                await testContext.TestMessageBus.Publish(new ApprenticeshipCreated2Event { Email = "bob" }));
        }

        [Then("an apprenticeship record is created")]
        public void ThenANewApprenticeshipRecordIsCreated()
        {
            var requests = testContext
                .Api
                .Server
                .FindLogEntries(
                    Request
                        .Create()
                        .WithPath(x => x.Contains("apprenticeships"))
                        .UsingPost()).AsEnumerable();

            requests.Should().NotBeEmpty();
        }
    }
}