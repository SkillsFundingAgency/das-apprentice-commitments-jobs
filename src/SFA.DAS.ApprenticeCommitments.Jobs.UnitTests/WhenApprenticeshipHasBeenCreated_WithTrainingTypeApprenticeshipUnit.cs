using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Handlers.CommitmentsEventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipHasBeenCreated_WithTrainingTypeApprenticeshipUnit
    {
        [Test, AutoMoqData]
        public async Task Then_it_does_not_notify_apim(
            [Frozen] Mock<IEcsApi> api,
            CommitmentsEventHandler sut)
        {
            var evt = new ApprenticeshipCreatedEventWithStringTrainingType
            {
                ApprenticeshipId = 123,
                ContinuationOfId = null,
                TrainingType = "Apprenticeship Unit"
            };

            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.CreateApproval(It.IsAny<ApprovalCreated>()), Times.Never);
            api.Verify(m => m.UpdateApproval(It.IsAny<ApprovalUpdated>()), Times.Never);
        }

        private class ApprenticeshipCreatedEventWithStringTrainingType : ApprenticeshipCreatedEvent
        {
            public string TrainingType { get; set; } = string.Empty;
        }
    }
}

