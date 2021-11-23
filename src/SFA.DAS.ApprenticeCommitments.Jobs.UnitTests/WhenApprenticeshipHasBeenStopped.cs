using AutoFixture.AutoMoq;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.CommitmentsEventHandlers;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.DomainEvents;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.InternalMessages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Notifications.Messages.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using CmadApprenticeshipStoppedEvent = SFA.DAS.ApprenticeCommitments.Messages.Events.ApprenticeshipStoppedEvent;
using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    internal class WhenApprenticeshipHasBeenStopped
    {
        [Test, AutoMoqData]
        public async Task Create_timeout(
            StoppedApprenticeshipHandler sut,
            ApprenticeshipStoppedEvent evt)
        {
            var context = new TestableMessageHandlerContext();
            sut.Data = new StoppedApprenticeshipSagaData();
            await sut.Handle(evt, context);

            sut.Data.CommitmentsApprenticeshipId = evt.ApprenticeshipId;
            sut.Data.CommitmentsStoppedOn = evt.StopDate;
            context.TimeoutMessages.Should().Contain(x => x.Message is StoppedApprenticeshipTimeout);
        }

        [Test, AutoMoqData]
        public async Task Process_stop(
            StoppedApprenticeshipHandler sut,
            StoppedApprenticeshipSagaData data)
        {
            var context = new TestableMessageHandlerContext();
            sut.Data = data;
            await sut.Timeout(new StoppedApprenticeshipTimeout(), context);

            context.SentMessages.Should().Contain(x => x.Message is ProcessStoppedApprenticeship)
                .Which.Message.Should().BeEquivalentTo(new
                {
                    data.CommitmentsApprenticeshipId,
                    data.CommitmentsStoppedOn,
                });
        }

        [Test, AutoMoqData]
        public async Task Notify_apim(
            [Frozen] Mock<IEcsApi> api,
            ProcessStoppedApprenticeshipsHandler sut,
            ProcessStoppedApprenticeship evt)
        {
            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(x => x.StopApprenticeship(It.Is<ApprovalStopped>(p
                => p.CommitmentsApprenticeshipId == evt.CommitmentsApprenticeshipId)));
        }

        [Test, AutoMoqData]
        public async Task Abandon_stop_if_apprenticeship_is_continued(
            StoppedApprenticeshipHandler sut,
            ApprenticeshipStoppedEvent stopEvent,
            ApprenticeshipCreatedEvent continueEvent)
        {
            // Given
            sut.Data = new StoppedApprenticeshipSagaData();
            continueEvent.ApprenticeshipId = stopEvent.ApprenticeshipId;

            // When
            var context = new TestableMessageHandlerContext();
            await sut.Handle(stopEvent, context);
            await sut.Handle(continueEvent, context);

            // Then
            sut.Completed.Should().BeTrue();
        }

        [Test, TestAutoData]
        public async Task Send_email(
            [Frozen] Api.Apprentice apprentice,
            [Frozen] ApplicationSettings settings,
            StoppedApprenticeshipEventHandler sut,
            CmadApprenticeshipStoppedEvent evt
            )
        {
            var context = new TestableMessageHandlerContext();
            await sut.Handle(evt, context);

            context.SentMessages
                .Should().Contain(x => x.Message is SendEmailCommand)
                .Which.Message.Should().BeEquivalentTo(new
                {
                    TemplateId = settings.Notifications.ApprenticeshipStopped.ToString(),
                    RecipientsAddress = apprentice.Email,
                    Tokens = new Dictionary<string, string>
                    {
                        { "FirstName", apprentice.FirstName },
                        { "FamilyName", apprentice.LastName },
                        { "ConfirmApprenticeshipUrl", settings.ApprenticeWeb.ConfirmApprenticeshipUrl.ToString() },
                        { "CourseName", evt.CourseName },
                        { "EmployerName", evt.EmployerName },
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
                        s.Templates.Add("ApprenticeshipStopped", Guid.NewGuid().ToString());
                    }));
                return fixture;
            }
        }
    }
}