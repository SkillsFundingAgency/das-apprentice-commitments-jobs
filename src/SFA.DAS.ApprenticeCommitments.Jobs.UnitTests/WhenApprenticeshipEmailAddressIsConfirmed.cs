using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using System.Threading.Tasks;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Api;
using SFA.DAS.ApprenticeCommitments.Messages.Events;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipEmailAddressIsConfirmed
    {
        [Test, AutoMoqData]
        public async Task Then_notify_apim(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipCommitmentsJobsHandler sut,
            ApprenticeshipEmailAddressConfirmedEvent evt)
        {
            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.SetEmailAddressConfirmed(evt.ApprenticeId, It.Is<ApprenticeshipEmailAddressConfirmation>(n =>
                n.CommitmentsApprenticeshipId == evt.CommitmentsApprenticeshipId)));
        }
    }
}