using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Apprentice.LoginService.Messages.Commands;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipUpdateEmailAddressCommandHandler : IHandleMessages<UpdateEmailAddressCommand>
    {
        private readonly IEcsApi _api;
        private readonly ILogger<ApprenticeshipUpdateEmailAddressCommandHandler> _logger;

        public ApprenticeshipUpdateEmailAddressCommandHandler(IEcsApi api, ILogger<ApprenticeshipUpdateEmailAddressCommandHandler> logger)
        {
            _api = api;
            _logger = logger;
        }

        public Task Handle(UpdateEmailAddressCommand message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received {nameof(UpdateEmailAddressCommand)} for apprentice {message.ApprenticeId}");
            var requestBody = new JsonPatchDocument<Api.Apprentice>().Replace(x => x.Email, message.NewEmailAddress);
            return _api.UpdateApprentice(message.ApprenticeId, requestBody);
        }
    }
}