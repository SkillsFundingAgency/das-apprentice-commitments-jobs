using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.DomainEvents;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.InternalMessages.Commands;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class TestInvitationReminder
    {
        [Test, AutoMoqData]
        public async Task Send_reminder_emails_with_default_cutoff(
            [Frozen] ApplicationSettings config,
            [Frozen] Mock<IEcsApi> api,
            SendInvitationRemindersHandler sut)
        {
            config.SendRemindersAfterThisNumberDays = 0;

            await sut.Run(null, Mock.Of<ExecutionContext>(), Mock.Of<ILogger>());

            api.Verify(m => m.GetReminderRegistrations(
                It.Is<DateTime>(
                    n => n.IsCloseTo(DateTime.UtcNow.AddDays(-7), TimeSpan.FromSeconds(1)))));
        }

        [Test, AutoMoqData]
        public async Task Send_reminder_emails_with_configured_cutoff(
            [Frozen] ApplicationSettings config,
            [Frozen] Mock<IEcsApi> api, SendInvitationRemindersHandler sut)
        {
            config.SendRemindersAfterThisNumberDays = 99;

            await sut.Run(null, Mock.Of<ExecutionContext>(), Mock.Of<ILogger>());

            api.Verify(m => m.GetReminderRegistrations(
                It.Is<DateTime>(
                    n => n.IsCloseTo(DateTime.UtcNow.AddDays(-99), TimeSpan.FromSeconds(1)))));
        }

        [Test, AutoMoqData]
        public async Task Send_reminder_emails_with_negative_cutoff_uses_default(
            [Frozen] ApplicationSettings config,
            [Frozen] Mock<IEcsApi> api, SendInvitationRemindersHandler sut)
        {
            config.SendRemindersAfterThisNumberDays = -15;

            await sut.Run(null, Mock.Of<ExecutionContext>(), Mock.Of<ILogger>());

            api.Verify(m => m.GetReminderRegistrations(
                It.Is<DateTime>(
                    n => n.IsCloseTo(DateTime.UtcNow.AddDays(-7), TimeSpan.FromSeconds(1)))));
        }

        [Test, AutoMoqData]
        public async Task Queues_reminder_for_registration(
            [Frozen] Mock<IFunctionEndpoint> endpoint,
            [Frozen] Registration registration,
            SendInvitationRemindersHandler sut)
        {
            await sut.Run(null, Mock.Of<ExecutionContext>(), Mock.Of<ILogger>());

            endpoint.Verify(e => e.Send(
                It.Is<RemindApprenticeCommand>(c => c.RegistrationId == registration.RegistrationId),
                It.IsAny<SendOptions>(),
                It.IsAny<ExecutionContext>(),
                It.IsAny<ILogger>()));
        }

        [Test, AutoMoqData]
        public async Task Send_reminder_invite_command_with_correct_details(
            [Frozen] ApplicationSettings settings,
            [Frozen] Registration registration,
            RemindApprenticeCommandHandler sut,
            RemindApprenticeCommand evt,
            Guid emailTemplateId)
        {
            evt.RegistrationId = registration.RegistrationId;
            settings.Notifications.Templates.Add("ApprenticeSignUp", emailTemplateId.ToString());

            var context = new TestableMessageHandlerContext();
            await sut.Handle(evt, context);

            var url = $"{settings.ApprenticeWeb.StartPageUrl}?Register={evt.RegistrationId}";

            context.SentMessages
                .Should().Contain(x => x.Message is SendEmailCommand)
                .Which.Message.Should().BeEquivalentTo(new
                {
                    TemplateId = settings.Notifications.ApprenticeSignUp.ToString(),
                    RecipientsAddress = registration.Email,
                    Tokens = new Dictionary<string, string>
                    {
                        { "GivenName", registration.FirstName },
                        { "CreateAccountLink", url },
                        { "LoginLink", url },
                    }
                });
        }
    }

    public static class DateCloseness
    {
        public static bool IsCloseTo(this DateTime a, DateTime b, TimeSpan resolution)
            => a.Subtract(resolution) < b && a.Add(resolution) > b;
    }
}