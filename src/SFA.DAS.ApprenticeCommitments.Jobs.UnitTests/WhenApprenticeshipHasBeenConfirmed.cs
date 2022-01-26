using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Handlers.DomainEvents;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipHasBeenConfirmed
    {
        [Test, TestAutoData]
        public async Task Then_email_the_apprentice(
            [Frozen] Mock<IEcsApi> api,
            [Frozen] UrlConfiguration _,
            [Frozen] ApplicationSettings settings,
            ApprenticeshipConfirmedEventHandler sut,
            ApprenticeshipConfirmationConfirmedEvent evt,
            Api.Apprentice apprentice)
        {
            evt.ApprenticeId = apprentice.Id;
            api.Setup(x => x.GetApprentice(evt.ApprenticeId)).ReturnsAsync(apprentice);

            var context = new TestableMessageHandlerContext();
            await sut.Handle(evt, context);

            context.SentMessages
                .Should().Contain(x => x.Message is SendEmailCommand)
                .Which.Message.Should().BeEquivalentTo(new
                {
                    TemplateId = settings.Notifications.ApprenticeshipConfirmed.ToString(),
                    RecipientsAddress = apprentice.Email,
                    Tokens = new Dictionary<string, string>
                    {
                        { "FirstName", apprentice.FirstName },
                        { "MyApprenticeshipUrl", settings.ApprenticeWeb.StartPageUrl.ToString() },
                        { "SurveyLink", settings.SurveyLink.ToString() },
                    }
                });
        }

        public class TestAutoDataAttribute : AutoDataAttribute
        {
            public TestAutoDataAttribute()
                : base(() => Fixture())
            {
            }

            private static IFixture Fixture()
            {
                var fixture = new Fixture();
                fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
                fixture.Customize<NotificationConfiguration>(c => c
                    .Without(s => s.Templates)
                    .Do(s =>
                    {
                        s.Templates.Add("ApprenticeshipConfirmed", Guid.NewGuid().ToString());
                    }));
                return fixture;
            }
        }
    }
}