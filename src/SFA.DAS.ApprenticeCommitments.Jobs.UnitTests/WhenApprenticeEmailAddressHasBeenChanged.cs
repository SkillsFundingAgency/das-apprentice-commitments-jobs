using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeAccounts.Messages.Events;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.DomainEvents;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeEmailAddressHasBeenChanged
    {
        [Test, AutoMoqData]
        public async Task Then_publish_ApprenticeshipEmailAddressChangedEvent(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeEmailAddressChangedHandler sut,
            ApprenticeEmailAddressChanged evt,
            Apprenticeship apprenticeship)
        {
            api.Setup(x => x.GetApprenticeships(evt.ApprenticeId)).ReturnsAsync(new ApprenticeshipsWrapper
            {
                Apprenticeships = new[] { apprenticeship }.ToList()
            });

            var context = new TestableMessageHandlerContext();
            await sut.Handle(evt, context);

            context.PublishedMessages
                .Should().ContainEquivalentOf(new
                {
                    Message = new
                    {
                        apprenticeship.ApprenticeId,
                        apprenticeship.CommitmentsApprenticeshipId
                    }
                })
                .Which.Message.Should().BeOfType<ApprenticeshipEmailAddressChangedEvent>();
        }

        [Test, AutoMoqData]
        public async Task Do_nothing_if_there_are_no_apprenticeships(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeEmailAddressChangedHandler sut,
            ApprenticeEmailAddressChanged evt)
        {
            api.Setup(x => x.GetApprenticeships(evt.ApprenticeId)).ReturnsAsync(new ApprenticeshipsWrapper
            {
                Apprenticeships = new List<Apprenticeship>()
            });

            await sut.Handle(evt, new TestableMessageHandlerContext());

            new TestableMessageHandlerContext().PublishedMessages.Should().BeEmpty();
        }

        [Test, AutoMoqData]
        public async Task Publish_ApprenticeshipEmailAddressChangedEvent_for_all_apprenticeship(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeEmailAddressChangedHandler sut,
            ApprenticeEmailAddressChanged evt,
            Apprenticeship[] apprenticeships)
        {
            apprenticeships[1].ApprovedOn = apprenticeships[0].ApprovedOn.AddDays(-10);
            apprenticeships[2].ApprovedOn = apprenticeships[0].ApprovedOn.AddDays(-20);
            api.Setup(x => x.GetApprenticeships(evt.ApprenticeId)).ReturnsAsync(new ApprenticeshipsWrapper
            {
                Apprenticeships = apprenticeships.ToList()
            });

            var context = new TestableMessageHandlerContext();
            await sut.Handle(evt, context);

            context.PublishedMessages
                .Should().ContainEquivalentOf(new
                {
                    Message = new
                    {
                        apprenticeships[0].ApprenticeId,
                        apprenticeships[0].CommitmentsApprenticeshipId
                    }
                })
                .And.ContainEquivalentOf(new
                {
                    Message = new
                    {
                        apprenticeships[1].ApprenticeId,
                        apprenticeships[1].CommitmentsApprenticeshipId
                    }
                })
                .And.ContainEquivalentOf(new
                {
                    Message = new
                    {
                        apprenticeships[2].ApprenticeId,
                        apprenticeships[2].CommitmentsApprenticeshipId
                    }
                })
                .Which.Message.Should().BeOfType<ApprenticeshipEmailAddressChangedEvent>();
        }
    }
}