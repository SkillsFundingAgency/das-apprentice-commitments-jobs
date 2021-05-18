using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipHasBeenCreated
    {
        [Test, AutoMoqData]
        public async Task Then_create_the_apprentice_record(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipCommitmentsJobsHandler sut,
            ApprenticeshipCreated2Event evt)
        {
            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.CreateApprentice(It.Is<ApprenticeshipCreated>(n =>
                n.ApprenticeshipId == evt.ApprenticeshipId &&
                n.Email == evt.Email &&
                n.EmployerName == evt.LegalEntityName &&
                n.CommitmentsApprovedOn == evt.CreatedOn)));
        }
    }
}