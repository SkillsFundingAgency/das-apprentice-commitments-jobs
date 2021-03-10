using FluentAssertions;
using NServiceBus.Transport;
using SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Newtonsoft.Json;
using NUnit.Framework.Internal.Commands;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Steps
{
    [Binding]
    public class NewApprenticeshipSteps : StepsBase
    {
        private Fixture _fixture;
        private ApprenticeshipCreated2Event _event;

        public NewApprenticeshipSteps(TestContext testContext) : base(testContext)
        {
            _fixture = new Fixture();
            _event = _fixture.Build<ApprenticeshipCreated2Event>().With(x=>x.Email, "bob@bobs.com").Create();
        }

        [Given(@"outer api is available")]
        public void GivenOuterApiIsAvailable()
        {
            testContext.Api.MockServer.Given(
                    Request.Create()
                        .WithPath("/apprenticeships")
                        .UsingPost())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode((int)HttpStatusCode.Accepted));
        }

        [When("an apprenticeship is approved")]
        public async Task WhenAnApprenticeshipIsApproved()
        {

            await testContext.WaitFor<MessageContext>(async () =>
                await testContext.TestMessageBus.Publish(_event));
        }

        [Then("an apprenticeship record is created")]
        public void ThenANewApprenticeshipRecordIsCreated()
        {
            var logs = testContext.Api.MockServer.LogEntries;
            logs.Should().HaveCount(1);
            var request = JsonConvert.DeserializeObject<Apprenticeship>(logs.First().RequestMessage.Body);

            request.Should().NotBeNull();
            request.ApprenticeshipId.Should().Be(_event.ApprenticeshipId);
            request.Email.Should().Be(_event.Email);
            request.AccountLegalEntityId.Should().Be(_event.AccountLegalEntityId);
            request.EmployerAccountId.Should().Be(_event.AccountId);
            request.EmployerName.Should().Be(_event.LegalEntityName);
        }
    }
}