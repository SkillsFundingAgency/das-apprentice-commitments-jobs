using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeEmailHasBeenUpdated
    {
        [Test, AutoMoqData]
        public async Task Then_notify_apim(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipCommitmentsJobsHandler sut,
            EmailChangedEvent evt)
        {
            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.UpdateApprenticeEmail(evt.ApprenticeId, It.Is<EmailUpdate>(n =>
                n.Email == evt.NewEmailAddress)));
        }
    }
}