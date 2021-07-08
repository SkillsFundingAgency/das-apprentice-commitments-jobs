using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class TestInvitationReminder
    {
        [Test, AutoMoqData]
        public async Task Send_reminder_emails_with_default_cutoff([Frozen] Mock<IEcsApi> api, SendInvitationRemindersHandler sut)
        {
            await sut.Run(null, Mock.Of<ILogger>());

            api.Verify(m => m.SendInvitationReminders(
                It.Is<SendInvitationRemindersRequest>(
                    n => n.InvitationCutOffTime.IsCloseTo(DateTime.UtcNow.AddDays(-7), TimeSpan.FromSeconds(1)))));
        }

        [Test, AutoMoqData]
        public async Task Send_reminder_emails_with_configured_cutoff([Frozen] Mock<IConfiguration> config,
            [Frozen] Mock<IEcsApi> api, SendInvitationRemindersHandler sut)
        {
            config.Setup(x => x["SendRemindersAfterThisNumberDays"]).Returns("99");

            await sut.Run(null, Mock.Of<ILogger>());

            api.Verify(m => m.SendInvitationReminders(
                It.Is<SendInvitationRemindersRequest>(
                    n => n.InvitationCutOffTime.IsCloseTo(DateTime.UtcNow.AddDays(-99), TimeSpan.FromSeconds(1)))));
        }

        [Test, AutoMoqData]
        public async Task Send_reminder_emails_with_negative_cutoff_uses_default([Frozen] Mock<IConfiguration> config,
            [Frozen] Mock<IEcsApi> api, SendInvitationRemindersHandler sut)
        {
            config.Setup(x => x["SendRemindersAfterThisNumberDays"]).Returns("-15");

            await sut.Run(null, Mock.Of<ILogger>());

            api.Verify(m => m.SendInvitationReminders(
                It.Is<SendInvitationRemindersRequest>(
                    n => n.InvitationCutOffTime.IsCloseTo(DateTime.UtcNow.AddDays(-7), TimeSpan.FromSeconds(1)))));
        }
    }

    public static class DateCloseness
    {
        public static bool IsCloseTo(this DateTime a, DateTime b, TimeSpan resolution)
            => a.Subtract(resolution) < b && a.Add(resolution) > b;
    }
}