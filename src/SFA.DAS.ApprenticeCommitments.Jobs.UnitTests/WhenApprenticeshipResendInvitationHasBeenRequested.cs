﻿using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.CommitmentsEventHandlers;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipResendInvitationHasBeenRequested
    {
        [Test, AutoMoqData]
        public async Task Then_SendEmailCommand_for_apprentice_signup_when_apprentice_is_not_assigned(
            [Frozen] Mock<IEcsApi> api,
            [Frozen] ApplicationSettings applicationSettings,
            Guid emailTemplateId,
            ApprenticeshipResendInvitationEventHandler sut,
            ApprenticeshipResendInvitationEvent evt,
            Registration registration)
        {
            registration.ApprenticeId = null;
            api.Setup(x => x.GetApprovalsRegistration(evt.ApprenticeshipId)).ReturnsAsync(registration);

            applicationSettings.Notifications.Templates.Add("ApprenticeSignUp", emailTemplateId.ToString());
            var link = $"{applicationSettings.ApprenticeWeb.StartPageUrl}?Register={registration.RegistrationId}";

            var context = new TestableMessageHandlerContext();
            await sut.Handle(evt, context);

            context.SentMessages
                .Should().Contain(x => x.Message is SendEmailCommand)
                .Which.Message.Should().BeEquivalentTo(new
                {
                    TemplateId = applicationSettings.Notifications.ApprenticeSignUp.ToString(),
                    RecipientsAddress = registration.Email,
                    Tokens = new Dictionary<string, string>
                    {
                        { "GivenName", registration.FirstName },
                        { "CreateAccountLink", link },
                        { "LoginLink",  link },
                    }
                });
        }

        [Test, AutoMoqData]
        public async Task Then_do_not_SendEmailCommand_if_apprentice_has_been_assigned(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipResendInvitationEventHandler sut,
            ApprenticeshipResendInvitationEvent evt,
            Registration registration)
        {
            api.Setup(x => x.GetApprovalsRegistration(evt.ApprenticeshipId)).ReturnsAsync(registration);

            var context = new TestableMessageHandlerContext();
            await sut.Handle(evt, context);

            context.SentMessages
                .Should().BeEmpty();
        }

        [Test, AutoMoqData]
        public async Task Then_do_not_SendEmailCommand_if_no_Registration_record_found(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipResendInvitationEventHandler sut,
            ApprenticeshipResendInvitationEvent evt,
            Registration registration)
        {
            var context = new TestableMessageHandlerContext();
            await sut.Handle(evt, context);

            context.SentMessages
                .Should().BeEmpty();
        }
    }
}