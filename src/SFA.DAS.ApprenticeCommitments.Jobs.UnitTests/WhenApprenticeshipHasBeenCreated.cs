using NSubstitute;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;
using Xunit;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeshipHasBeenCreated
    {
        [Fact]
        public async Task Then_create_the_apprentice_record()
        {
            var api = Substitute.For<IEcsApi>();
            var evt = new ApprenticeshipCreatedEvent();
            await ApprenticeshipCreatedHandler.RunEvent(evt, api);
            await api.Received().CreateApprentice("bob");
        }
    }
}