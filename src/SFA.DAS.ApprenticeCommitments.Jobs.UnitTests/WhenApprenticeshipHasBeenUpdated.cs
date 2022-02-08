using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Handlers.CommitmentsEventHandlers;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipHasBeenUpdated
    {
        [Test, AutoMoqData]
        public async Task Then_notify_apim(
            [Frozen] Mock<IEcsApi> api,
            CommitmentsEventHandler sut,
            ApprenticeshipUpdatedApprovedEvent evt)
        {
            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.UpdateApproval(It.Is<ApprovalUpdated>(n =>
                n.CommitmentsContinuedApprenticeshipId == null &&
                n.CommitmentsApprenticeshipId == evt.ApprenticeshipId &&
                n.CommitmentsApprovedOn == evt.ApprovedOn)));
        }
    }
}