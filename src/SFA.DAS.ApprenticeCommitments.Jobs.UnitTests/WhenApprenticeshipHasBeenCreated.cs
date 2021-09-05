using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Policy;
using FluentAssertions;
using SFA.DAS.Notifications.Messages.Commands;
using System.Runtime;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipHasBeenCreated
    {
        private readonly Fixture _fixture = new Fixture();

        [Test, AutoMoqData]
        public async Task And_it_is_a_new_apprenticeship_Then_create_the_apprentice_record(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipCommitmentsJobsHandler sut)
        {
            api.Setup(x => x.CreateApprentice(It.IsAny<ApprenticeshipCreated>()))
               .ReturnsAsync(_fixture.Create<CreateRegistrationResponse>());

            var evt = _fixture.Build<ApprenticeshipCreatedEvent>()
               .Without(p => p.ContinuationOfId)
               .Create();

            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.CreateApprentice(It.Is<ApprenticeshipCreated>(n =>
                n.CommitmentsApprenticeshipId == evt.ApprenticeshipId &&
                n.EmployerName == evt.LegalEntityName &&
                n.CommitmentsApprovedOn == evt.CreatedOn)));
        }

        [Test, AutoMoqData]
        public async Task And_it_is_a_continuation_apprenticeship_Then_update_the_apprentice_record(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipCommitmentsJobsHandler sut)
        {
            var continuationId = _fixture.Create<long>();

            var evt = _fixture.Build<ApprenticeshipCreatedEvent>()
                .With(p => p.ContinuationOfId, continuationId)
                .Create();

            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.UpdateApprenticeship(It.Is<ApprenticeshipUpdated>(n =>
                n.CommitmentsContinuedApprenticeshipId == continuationId &&
                n.CommitmentsApprenticeshipId == evt.ApprenticeshipId &&
                n.CommitmentsApprovedOn == evt.CreatedOn)));
        }
    }

    public class WhenRegistationHasBeenSaved
    {
        [Test, TestAutoData]
        public async Task Then_email_the_apprentice(
            [Frozen] Mock<IEcsApi> api,
            [Frozen] ApplicationSettings settings,
            ApprenticeshipRegisteredEventHandler sut,
            ApprenticeshipRegisteredEvent evt,
            Registration registration)
        {
            evt.RegistrationId = registration.RegistrationId;
            api.Setup(x => x.GetRegistration(evt.RegistrationId)).ReturnsAsync(registration);
            
            var context = new TestableMessageHandlerContext();
            await sut.Handle(evt, context);

            var url = $"{settings.ApprenticeLoginApi.RedirectUrl}?Register={registration.RegistrationId}";
            context.SentMessages
                .Should().Contain(x => x.Message is SendEmailCommand)
                .Which.Message.Should().BeEquivalentTo(new
                {
                    TemplateId = settings.Notifications.ApprenticeSignUpInvitation.ToString(),
                    RecipientsAddress = registration.Email,
                    Tokens = new Dictionary<string, string>
                    {
                        { "GivenName", registration.FirstName },
                        { "CreateAccountLink", url },
                        { "LoginLink", url },
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
                        s.Templates.Add("ApprenticeshipChangedEmail", Guid.NewGuid().ToString());
                        s.Templates.Add("ApprenticeSignUpInvitation", Guid.NewGuid().ToString());
                    }));
                return fixture;
            }
        }
    }
}