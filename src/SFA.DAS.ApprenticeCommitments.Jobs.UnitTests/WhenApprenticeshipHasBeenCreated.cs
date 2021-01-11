using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using NSubstitute;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;
using Xunit;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipHasBeenCreated
    {
        [Theory, DomainAutoData]
        public async Task Then_create_the_apprentice_record(
            [Frozen] IEcsApi api,
            ApprenticeshipCreatedHandler sut,
            ApprenticeshipCreatedEvent2 evt)
        {
            await sut.RunEvent(evt);
            await api.Received().CreateApprentice(evt.Email);
        }

        [Theory(Skip = "Not implemented yet")]
        [DomainAutoData]
        public async Task Logs_when_event_without_email_is_received(
            [Frozen] IEcsApi api,
            ApprenticeshipCreatedHandler sut,
            ApprenticeshipCreatedEvent2 evt)
        {
            await sut.RunEvent(evt);
            // assert
        }

        public class DomainAutoDataAttribute : AutoDataAttribute
        {
            public DomainAutoDataAttribute() : base(() => Customise())
            {
            }

            private static IFixture Customise()
            {
                var fixture = new Fixture();
                fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
                return fixture;
            }
        }
    }
}