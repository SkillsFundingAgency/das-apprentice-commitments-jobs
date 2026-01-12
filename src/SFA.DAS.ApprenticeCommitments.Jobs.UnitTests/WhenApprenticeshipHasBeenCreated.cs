using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Handlers.CommitmentsEventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipHasBeenCreated
    {
        [Test, AutoMoqData]
        public async Task And_it_is_a_new_apprenticeship_Then_create_the_apprentice_record(
            [Frozen] Mock<IEcsApi> api,
            CommitmentsEventHandler sut,
            ApprenticeshipCreatedEvent message)
        {
            message.TrainingType = ProgrammeType.Standard;
            message.ContinuationOfId = null;

            await sut.Handle(message, new TestableMessageHandlerContext());

            api.Verify(m => m.CreateApproval(It.IsAny<ApprovalCreated>()), Times.Once);
            api.Verify(m => m.UpdateApproval(It.IsAny<ApprovalUpdated>()), Times.Never);
        }

        [Test, AutoMoqData]
        public async Task And_it_is_a_continuation_apprenticeship_Then_update_the_apprentice_record(
            [Frozen] Mock<IEcsApi> api,
            CommitmentsEventHandler sut,
            ApprenticeshipCreatedEvent message)
        {
            message.TrainingType = ProgrammeType.Standard;
            message.ContinuationOfId = 999;

            await sut.Handle(message, new TestableMessageHandlerContext());

            api.Verify(m => m.UpdateApproval(It.IsAny<ApprovalUpdated>()), Times.Once);
            api.Verify(m => m.CreateApproval(It.IsAny<ApprovalCreated>()), Times.Never);
        }
    }
}