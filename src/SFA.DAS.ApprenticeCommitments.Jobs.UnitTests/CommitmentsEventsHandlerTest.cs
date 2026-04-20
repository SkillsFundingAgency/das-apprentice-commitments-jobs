using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Handlers.CommitmentsEventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests.CommitmentsEventHandlers
{
    public class CommitmentsEventsHandlerTest
    {
        [Test, TestAutoData]
        public async Task Then_ApprenticeshipCreatedEvent_is_ignored_when_learning_type_not_supported(
            [Frozen] Mock<IEcsApi> api,
            [Frozen] Mock<ILogger<CommitmentsEventHandler>> logger,
            CommitmentsEventHandler sut,
            ApprenticeshipCreatedEventWithLearningType message)
        {
            message.LearningType = "SomethingElse";

            var context = new TestableMessageHandlerContext();

            await sut.Handle(message, context);

            api.Verify(x => x.CreateApproval(It.IsAny<ApprovalCreated>()), Times.Never);
            api.Verify(x => x.UpdateApproval(It.IsAny<ApprovalUpdated>()), Times.Never);

            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Ignoring ApprenticeshipCreatedEvent")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test, TestAutoData]
        public async Task Then_ApprenticeshipCreatedEvent_calls_CreateApproval_when_not_a_continuation_and_learning_type_supported(
            [Frozen] Mock<IEcsApi> api,
            CommitmentsEventHandler sut,
            ApprenticeshipCreatedEventWithLearningType message)
        {
            message.LearningType = "Apprenticeship";
            message.ContinuationOfId = null;

            var context = new TestableMessageHandlerContext();

            await sut.Handle(message, context);

            api.Verify(x => x.CreateApproval(It.IsAny<ApprovalCreated>()), Times.Once);
            api.Verify(x => x.UpdateApproval(It.IsAny<ApprovalUpdated>()), Times.Never);
        }

        [Test, TestAutoData]
        public async Task Then_ApprenticeshipCreatedEvent_calls_UpdateApproval_when_a_continuation_and_learning_type_supported(
            [Frozen] Mock<IEcsApi> api,
            CommitmentsEventHandler sut,
            ApprenticeshipCreatedEventWithLearningType message)
        {
            message.LearningType = "FoundationApprenticeship";
            message.ContinuationOfId = 12345;

            var context = new TestableMessageHandlerContext();

            await sut.Handle(message, context);

            api.Verify(x => x.UpdateApproval(It.IsAny<ApprovalUpdated>()), Times.Once);
            api.Verify(x => x.CreateApproval(It.IsAny<ApprovalCreated>()), Times.Never);
        }

        [Test, TestAutoData]
        public async Task Then_ApprenticeshipUpdatedApprovedEvent_is_ignored_when_learning_type_not_supported(
            [Frozen] Mock<IEcsApi> api,
            [Frozen] Mock<ILogger<CommitmentsEventHandler>> logger,
            CommitmentsEventHandler sut,
            ApprenticeshipUpdatedApprovedEventWithLearningType message)
        {
            message.LearningType = "NotSupported";

            var context = new TestableMessageHandlerContext();

            await sut.Handle(message, context);

            api.Verify(x => x.UpdateApproval(It.IsAny<ApprovalUpdated>()), Times.Never);

            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Ignoring ApprenticeshipUpdatedApprovedEvent")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test, TestAutoData]
        public async Task Then_ApprenticeshipUpdatedApprovedEvent_calls_UpdateApproval_when_learning_type_supported(
            [Frozen] Mock<IEcsApi> api,
            CommitmentsEventHandler sut,
            ApprenticeshipUpdatedApprovedEventWithLearningType message)
        {
            message.LearningType = "Foundation Apprenticeship";

            var context = new TestableMessageHandlerContext();

            await sut.Handle(message, context);

            api.Verify(x => x.UpdateApproval(It.IsAny<ApprovalUpdated>()), Times.Once);
        }

        public class ApprenticeshipCreatedEventWithLearningType : ApprenticeshipCreatedEvent
        {
            public string LearningType { get; set; }
        }

        public class ApprenticeshipUpdatedApprovedEventWithLearningType : ApprenticeshipUpdatedApprovedEvent
        {
            public string LearningType { get; set; }
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
                return fixture;
            }
        }
    }
}
