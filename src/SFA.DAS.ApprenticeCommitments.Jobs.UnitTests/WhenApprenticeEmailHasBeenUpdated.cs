using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.Apprentice.LoginService.Messages.Commands;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class WhenApprenticeEmailHasBeenUpdated
    {
        [Test, AutoMoqData]
        public async Task Then_notify_apim(
            [Frozen] Mock<IEcsApi> api,
            ApprenticeshipUpdateEmailAddressCommandHandler sut,
            UpdateEmailAddressCommand evt)
        {
            await sut.Handle(evt, new TestableMessageHandlerContext());

            VerifyPatchRequestIsConstructedAsExpected(api, evt);
        }

        private static void VerifyPatchRequestIsConstructedAsExpected(Mock<IEcsApi> api, UpdateEmailAddressCommand evt)
        {
            api.Verify(m => m.UpdateApprentice(evt.ApprenticeId, It.Is<JsonPatchDocument<Api.Apprentice>>(n =>
                n.Operations.Count == 1 && n.Operations[0].OperationType == OperationType.Replace &&
                n.Operations[0].path == "/Email" && (string) n.Operations[0].value == evt.NewEmailAddress)));
        }
    }
}