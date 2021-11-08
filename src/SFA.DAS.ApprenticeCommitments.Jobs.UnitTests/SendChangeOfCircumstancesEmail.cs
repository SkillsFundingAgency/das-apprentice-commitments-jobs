using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.DomainEvents;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class SendChangeOfCircumstancesEmail
    {
        [Test, AutoMoqData]
        public async Task Send_email_with_template_fields(
            [Frozen] Api.Apprentice apprentice,
            [Frozen] ApplicationSettings settings,
            ApprenticeshipChangedEventHandler sut,
            ApprenticeshipChangedEvent evt,
            Guid emailTemplateId)
        {
            settings.Notifications.Templates.Add("ApprenticeshipChanged", emailTemplateId.ToString());

            var context = new TestableMessageHandlerContext();

            await sut.Handle(evt, context);

            context.SentMessages
                .Should().Contain(x => x.Message is SendEmailCommand)
                .Which.Message.Should().BeEquivalentTo(new
                {
                    TemplateId = emailTemplateId.ToString(),
                    RecipientsAddress = apprentice.Email,
                    Tokens = new Dictionary<string, string>
                    {
                        { "GivenName", apprentice.FirstName },
                        { "FamilyName", apprentice.LastName },
                        { "ConfirmApprenticeshipUrl", settings.ApprenticeWeb.ConfirmApprenticeshipUrl.ToString() },
                    }
                });
        }

        [Test, AutoData]
        public void Confirm_apprenticeship_url_contains_subdomain(UrlConfiguration sut)
        {
            sut.ConfirmApprenticeshipUrl.ToString().ToLower().Should().Be(
                $"{sut.BaseUrl.Scheme}://confirm.{sut.BaseUrl.Host}/apprenticeships");
        }

        [Test, AutoMoqData]
        public async Task Missing_configuration_throws_exception(
        [Frozen] ApplicationSettings settings,
        ApprenticeshipChangedEventHandler sut,
        ApprenticeshipChangedEvent evt)
        {
            settings.Notifications.Templates.Clear();

            await sut
                .Invoking(s => s.Handle(evt, new TestableMessageHandlerContext()))
                .Should().ThrowAsync<Exception>().WithMessage("Missing configuration `Notifications:Templates:ApprenticeshipChanged`");
        }

        [Test, TestAutoData]
        public async Task Change_within_24_hours_of_previous_revision_does_not_send_email(
            [Frozen] ApprenticeshipHistory apprenticeship,
            ApprenticeshipChangedEventHandler sut,
            ApprenticeshipChangedEvent evt)
        {
            apprenticeship.Revisions[2].ApprovedOn =
                apprenticeship.Revisions[1].ApprovedOn.AddHours(23);

            var context = new TestableMessageHandlerContext();

            await sut.Handle(evt, context);

            context.SentMessages.Should().NotContain(x => x.Message is SendEmailCommand);
        }

        [Test, TestAutoData]
        public async Task Change_after_24_hours_of_previous_revision_sends_email(
            [Frozen] ApprenticeshipHistory apprenticeship,
            ApprenticeshipChangedEventHandler sut,
            ApprenticeshipChangedEvent evt)
        {
            apprenticeship.Revisions[2].ApprovedOn =
                apprenticeship.Revisions[1].ApprovedOn.AddHours(25);

            var context = new TestableMessageHandlerContext();
            await sut.Handle(evt, context);

            context.SentMessages.Should().Contain(x => x.Message is SendEmailCommand);
        }

        [Test, TestAutoData]
        public async Task Change_within_24_hours_of_viewed_previous_revision_sends_email(
            [Frozen] ApprenticeshipHistory apprenticeship,
            ApprenticeshipChangedEventHandler sut,
            ApprenticeshipChangedEvent evt)
        {
            apprenticeship.LastViewed =
                apprenticeship.Revisions[1].ApprovedOn.AddHours(1);
            apprenticeship.Revisions[2].ApprovedOn =
                apprenticeship.Revisions[1].ApprovedOn.AddHours(8);

            var context = new TestableMessageHandlerContext();
            await sut.Handle(evt, context);

            context.SentMessages
                .Where(x => x.Message is SendEmailCommand)
                .Should().HaveCount(1);
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
                fixture.Customize<ApplicationSettings>(c => c.
                    With(s => s.TimeToWaitBeforeChangeOfApprenticeshipEmail, TimeSpan.FromHours(24)));
                fixture.Customize<NotificationConfiguration>(c => c
                    .Without(s => s.Templates)
                    .Do(s => s.Templates.Add("ApprenticeshipChanged", Guid.NewGuid().ToString())));
                fixture.Customize<ApprenticeshipHistory>(c => c
                    .With(s => s.LastViewed, default(DateTime))
                    .With(s => s.Revisions,
                        fixture.CreateMany<ApprenticeshipRevision>().OrderBy(x => x.ApprovedOn).ToList()));
                return fixture;
            }
        }
    }
}