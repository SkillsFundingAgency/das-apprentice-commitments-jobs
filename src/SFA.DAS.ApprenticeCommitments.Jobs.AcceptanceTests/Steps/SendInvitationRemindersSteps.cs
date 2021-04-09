using System;
using FluentAssertions;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "SendInvitationReminders")]
    public class SendInvitationRemindersSteps
    {
        private readonly TestContext _context;
        private Fixture _fixture;

        public SendInvitationRemindersSteps(TestContext context)
        {
            _context = context;
            _fixture = new Fixture();

            _context.Api.MockServer.Given(
                    Request.Create()
                        .WithPath("/registrations/reminders")
                        .UsingPost())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode((int)HttpStatusCode.Accepted));
        }

        [Given(@"the reminder period is set")]
        public void GivenTheReminderPeriodIsSetToBeDays()
        {
        }

        [Given(@"the reminder is not set")]
        public void GivenTheReminderIsNotSet()
        {
        }

        [Given(@"at a specific time of the day")]
        public void GivenAtASpecificTimeOfTheDay()
        {
        }

        [When(@"the scheduled job starts")]
        public async Task WhenTheScheduledJobStarts()
        {
            await _context.FunctionsServer.SendInvitationRemindersHandler.Run(null, Mock.Of<ILogger>());
        }

        [Then(@"a request to send reminder emails is sent with expected value")]
        public void ThenARequestToSendReminderEmailsIsSentWithDays()
        {
            var logs = _context.Api.MockServer.LogEntries;
            logs.Should().HaveCount(1);
            var request = JsonConvert.DeserializeObject<SendInvitationRemindersRequest>(logs.First().RequestMessage.Body);

            request.Should().NotBeNull();
            request.InvitationCutOffTime.Should().BeBefore(DateTime.UtcNow.AddDays(-1 * TestContext.SendRemindersAfterThisNumberDays));
        }
    }
}