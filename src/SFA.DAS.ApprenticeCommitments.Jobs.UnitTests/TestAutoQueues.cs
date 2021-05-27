using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class TestAutoQueues
    {
        [Test]
        public async Task Create_queue_when_it_does_not_exist()
        {
            var m = new Mock<ManagementClient>("Endpoint=sb://bob.windows.net/;Authentication=Managed Identity;");
            m.Setup(x => x.QueueExistsAsync(QueueNames.ApprenticeshipCommitmentsJobs, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            await AutoSubscribeToQueues.CreateQueuesWithReflection(m.Object);

            m.Verify(x => x.CreateQueueAsync(QueueNames.ApprenticeshipCommitmentsJobs, It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Ignores_queue_when_it_does_already_exists()
        {
            var m = new Mock<ManagementClient>("Endpoint=sb://bob.windows.net/;Authentication=Managed Identity;");
            m.Setup(x => x.QueueExistsAsync(QueueNames.ApprenticeshipCommitmentsJobs, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await AutoSubscribeToQueues.CreateQueuesWithReflection(m.Object);

            m.Verify(x => x.CreateQueueAsync(QueueNames.ApprenticeshipCommitmentsJobs, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Create_error_queue_when_it_does_not_exist()
        {
            var m = new Mock<ManagementClient>("Endpoint=sb://bob.windows.net/;Authentication=Managed Identity;");
            m.Setup(x => x.QueueExistsAsync($"{QueueNames.ApprenticeshipCommitmentsJobs}-error", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            await AutoSubscribeToQueues.CreateQueuesWithReflection(m.Object);

            m.Verify(x => x.CreateQueueAsync($"{QueueNames.ApprenticeshipCommitmentsJobs}-error", It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Ignores_error_queue_when_it_does_already_exists()
        {
            var m = new Mock<ManagementClient>("Endpoint=sb://bob.windows.net/;Authentication=Managed Identity;");
            m.Setup(x => x.QueueExistsAsync($"{QueueNames.ApprenticeshipCommitmentsJobs}-error", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await AutoSubscribeToQueues.CreateQueuesWithReflection(m.Object);

            m.Verify(x => x.CreateQueueAsync($"{QueueNames.ApprenticeshipCommitmentsJobs}-error", It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Creates_subscription_when_it_does_not_exist()
        {
            var m = new Mock<ManagementClient>("Endpoint=sb://bob.windows.net/;Authentication=Managed Identity;");
            m.Setup(x => x.SubscriptionExistsAsync("bundle-1", "ApprenticeCommitments.Apprenticeship", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await AutoSubscribeToQueues.CreateQueuesWithReflection(m.Object);

            m.Verify(x => x.CreateSubscriptionAsync(
                It.Is<SubscriptionDescription>(d =>
                    d.TopicPath == "bundle-1" &&
                    d.SubscriptionName == "SFA.DAS.ApprenticeCommitments.Apprenticeship"),
                It.Is<RuleDescription>(r => r.Filter is FalseFilter),
                It.IsAny<CancellationToken>()));
        }

        [Test]
        public void Name_under_limit_when_common_parts_removed_is_used_whole()
        {
            var shortName = AzureQueueNameShortener.Shorten(typeof(ApprenticeshipCreatedEvent));
            Assert.That(shortName, Is.EqualTo("CommitmentsV2.ApprenticeshipCreatedEvent"));
        }

        [TestCase(typeof(VeryLongNamespaceThatItselfIsMoreThan50CharactersLong.ShortName), "ShortName.21AED97D")]
        [TestCase(typeof(VeryLongNamespaceThatItselfIsMoreThan50CharactersLong.AFairlyLongNameButUnder50Chars), "AFairlyLongNameButUnder50Chars.1B354C68")]
        [TestCase(typeof(VeryLongNamespaceThatItselfIsMoreThan50CharactersLong.ThisIsTheReallyReallyReallyLongNameThatIsOver50CharsItself), "ThisIsTheReallyReallyReallyLongNameThatIs.F215C1FC")]
        [TestCase(typeof(LongNamespaceButUnder50Chars.ShortName), "LongNamespaceButUnder50Chars.ShortName")]
        [TestCase(typeof(LongNamespaceButUnder50Chars.AFairlyLongNameButUnder50Chars), "AFairlyLongNameButUnder50Chars.D1B2D6DD")]
        [TestCase(typeof(LongNamespaceButUnder50Chars.ThisIsTheReallyReallyReallyLongNameThatIsOver50CharsItself), "ThisIsTheReallyReallyReallyLongNameThatIs.79B64E48")]
        public void Name_over_limit_when_common_parts_removed_is_used_shortened(Type type, string name)
        {
            var shortName = AzureQueueNameShortener.Shorten(type);
            Assert.That(shortName.Length, Is.LessThanOrEqualTo(50));
            Assert.That(shortName, Is.EqualTo(name));
        }
    }
}

namespace VeryLongNamespaceThatItselfIsMoreThan50CharactersLong
{
    public class ShortName { }
    public class AFairlyLongNameButUnder50Chars { }
    public class ThisIsTheReallyReallyReallyLongNameThatIsOver50CharsItself { }
}
namespace LongNamespaceButUnder50Chars
{
    public class ShortName { }
    public class AFairlyLongNameButUnder50Chars { }
    public class ThisIsTheReallyReallyReallyLongNameThatIsOver50CharsItself { }
}