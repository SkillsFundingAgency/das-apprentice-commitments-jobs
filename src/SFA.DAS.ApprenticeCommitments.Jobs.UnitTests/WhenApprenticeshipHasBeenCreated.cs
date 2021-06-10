using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;
using AutoFixture;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipHasBeenCreated
    {
        Fixture _fixture = new Fixture();

        [Test, AutoMoqData]
        public async Task And_it_is_a_new_apprenticeship_Then_create_the_apprentice_record(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipCommitmentsJobsHandler sut)
        {
            api.Setup(x => x.CreateApprentice(It.IsAny<ApprenticeshipCreated>()))
                .ReturnsAsync(_fixture.Build<CreateApprenticeshipResponse>()
                                      .With(x => x.ClientId, System.Guid.NewGuid().ToString()).Create());

            var evt = _fixture.Build<ApprenticeshipCreatedEvent>()
                .Without(p=>p.ContinuationOfId)
                .Create();

            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.CreateApprentice(It.Is<ApprenticeshipCreated>(n =>
                n.CommitmentsApprenticeshipId == evt.ApprenticeshipId &&
                n.EmployerName == evt.LegalEntityName &&
                n.CommitmentsApprovedOn == evt.CreatedOn)));
        }

        [Test, AutoMoqData]
        public async Task And_it_is_a_continuation_apprenticeship_Then_update_the_apprentice_record(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipCommitmentsJobsHandler sut)
        {
            var continuationId = _fixture.Create<long>();

            var evt = _fixture.Build<ApprenticeshipCreatedEvent>()
                .With(p => p.ContinuationOfId, continuationId)
                .Create();

            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.UpdateApprenticeship(It.Is<ApprenticeshipUpdated>(n =>
                n.CommitmentsContinuedApprenticeshipId == continuationId &&
                n.CommitmentsApprenticeshipId == evt.ApprenticeshipId &&
                n.CommitmentsApprovedOn == evt.CreatedOn)));
        }
    }
}