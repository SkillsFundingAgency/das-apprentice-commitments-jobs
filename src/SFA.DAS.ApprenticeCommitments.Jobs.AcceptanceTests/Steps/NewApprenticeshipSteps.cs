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

    [Binding]
    //[Scope(Tag = "messageBus")]
    public class MessageBus
    {
        private readonly TestContext context;

        public MessageBus(TestContext context) => this.context = context;

        [BeforeScenario(Order = 1)]
        public Task InitialiseMessageBus()
        {
            context.TestMessageBus = new TestMessageBus();
            context.Hooks.Add(new Hook<MessageContext>());
            return context.TestMessageBus.Start(/*_context.TestDirectory*/);
        }
    }

    [Binding]
    public class Functions
    {
        private readonly TestContext context;

        public Functions(TestContext context) => this.context = context;

        [BeforeScenario(Order = 3)]
        public async Task InitialiseFunctions()
        {
            context.FunctionsServer = new ApprenticeCommitmentApiTestServer(context);
            await context.FunctionsServer.Start();
        }
    }

    [Binding]
    public class EmployerIncentivesApi
    {
        private readonly TestContext context;

        public EmployerIncentivesApi(TestContext context) => this.context = context;

        [BeforeScenario(Order = 2)]
        public void InitialiseApi()
        {
            context.Api = new ApprenticeCommitmentsTestApi();
        }
    }
}