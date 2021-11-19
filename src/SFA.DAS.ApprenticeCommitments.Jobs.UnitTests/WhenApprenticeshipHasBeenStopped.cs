using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.CommitmentsEventHandlers;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.DomainEvents;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.InternalMessages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

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
            continueEvent.ApprenticeshipId = stopEvent.ApprenticeshipId;

            // When
            var context = new TestableMessageHandlerContext();
            await sut.Handle(stopEvent, context);
            await sut.Handle(continueEvent, context);

            // Then
            sut.Completed.Should().BeTrue();
        }
    }
}