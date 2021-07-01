using AutoFixture.NUnit3;
using FluentAssertions;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class SendChangeOfCircumstancesEmail
    {
        [Test, AutoMoqData]
        public async Task Send_email_with_template_fields(
            [Frozen] Apprentice apprentice,
            [Frozen] ApplicationSettings settings,
            ApprenticeshipChangedEventHandler sut,
            ApprenticeshipChangedEvent evt)
        {
            settings.Notifications.Templates.Add("ApprenticeshipChangedEmail", "99");

            var context = new TestableMessageHandlerContext();

            await sut.Handle(evt, context);

            var url = $"{settings.ApprenticeCommitmentsWeb.BaseUrl}/Apprenticeships";

            context.SentMessages
                .Should().Contain(x => x.Message is SendEmailCommand)
                .Which.Message.Should().BeEquivalentTo(new
                {
                    TemplateId = "99",
                    RecipientsAddress = apprentice.Email.ToString(),
                    Tokens = new Dictionary<string, string>
                    {
                        { "GivenName", apprentice.FirstName },
                        { "FamilyName", apprentice.LastName },
                        { "ConfirmApprenticeshipUrl", url },
                    }
                });
        }

        [Test, AutoMoqData]
        public void Missing_configuration_throws_exception(
        [Frozen] ApplicationSettings settings,
        ApprenticeshipChangedEventHandler sut,
        ApprenticeshipChangedEvent evt)
        {
            settings.Notifications.Templates.Clear();

            sut
                .Invoking(s => s.Handle(evt, new TestableMessageHandlerContext()))
                .Should().Throw<Exception>().WithMessage("Missing configuration `Notifications:Templates:ApprenticeshipChangedEmail`");
        }
    }
}