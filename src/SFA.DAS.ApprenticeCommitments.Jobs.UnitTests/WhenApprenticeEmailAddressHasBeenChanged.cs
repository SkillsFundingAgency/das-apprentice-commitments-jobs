using AutoFixture.NUnit3;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeAccounts.Messages.Events;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.DomainEvents;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeEmailAddressHasBeenChanged
    {
        [Test, AutoMoqData]
        public async Task Then_notify_apim(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeEmailAddressChangedHandler sut,
            ApprenticeEmailAddressChanged evt)
        {
            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.UpdateApprentice(evt.ApprenticeId, It.IsAny<JsonPatchDocument<Apprentice>>()));
        }
    }
}