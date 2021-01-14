using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipHasBeenCreated
    {
        [Test, DomainAutoData]
        public async Task Then_create_the_apprentice_record(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipCreatedHandler sut,
            ApprenticeshipCreatedEvent2 evt)
        {
            await sut.RunEvent(evt);
            api.Verify(m => m.CreateApprentice(It.Is<Apprenticeship>(n =>
                n.ApprenticeshipId == evt.ApprenticeshipId &&
                n.Email == evt.Email)));
        }

        //[Test]
        //[DomainAutoData]
        //public async Task Logs_when_event_without_email_is_received(
        //    [Frozen] IEcsApi api,
        //    ApprenticeshipCreatedHandler sut,
        //    ApprenticeshipCreatedEvent2 evt)
        //{
        //    await sut.RunEvent(evt);
        //    // assert
        //}

        public class DomainAutoDataAttribute : AutoDataAttribute
        {
            public DomainAutoDataAttribute() : base(() => Customise())
            {
            }

            private static IFixture Customise()
            {
                var fixture = new Fixture();
                fixture.Customize(new AutoMoqCustomization());
                return fixture;
            }
        }
    }
}