using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipHasBeenUpdated
    {
        [Test, AutoMoqData]
        public async Task Then_notify_apim(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipCommitmentsJobsHandler sut,
            ApprenticeshipUpdatedApprovedEvent evt)
        {
            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.UpdateApprenticeship(It.Is<ApprenticeshipUpdated>(n =>
                n.ContinuationOfCommitmentsApprenticeshipId == null &&
                n.CommitmentsApprenticeshipId == evt.ApprenticeshipId &&
                n.CommitmentsApprovedOn == evt.ApprovedOn)));
        }
    }
}