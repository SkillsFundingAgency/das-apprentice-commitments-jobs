using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.Apprentice.LoginService.Messages;
using SFA.DAS.Apprentice.LoginService.Messages.Commands;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipCommitmentsJobsHandler
        : IHandleMessages<ApprenticeshipCreatedEvent>
        , IHandleMessages<SendInvitationReply>
        , IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
        , IHandleMessages<UpdateEmailAddressCommand>
    {
        private readonly IEcsApi _api;
        private readonly ILogger<ApprenticeshipCommitmentsJobsHandler> _logger;
        private readonly LoginServiceOptions _nServiceBusOptions;

        public ApprenticeshipCommitmentsJobsHandler(
            IEcsApi api,
            ILogger<ApprenticeshipCommitmentsJobsHandler> logger,
            LoginServiceOptions nServiceBusOptions
            )
        {
            _api = api;
            _logger = logger;
            _nServiceBusOptions = nServiceBusOptions;
        }

        public async Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            if (message.ContinuationOfId.HasValue)
            {
                await _api.UpdateApprenticeship(message.ToApprenticeshipUpdated());
            }
            else
            {
                var res = await _api.CreateApprentice(message.ToApprenticeshipCreated());

                _logger.LogInformation($"CreateApprentice returned {JsonConvert.SerializeObject(res)}");
                _logger.LogInformation($"_nServiceBusOptions {JsonConvert.SerializeObject(_nServiceBusOptions)}");

                if (res != null)
                {
                    var invite = new SendInvitation()
                    {
                        ClientId = _nServiceBusOptions.IdentityServerClientId,
                        SourceId = res.SourceId.ToString(),
                        Email = res.Email,
                        GivenName = res.GivenName,
                        FamilyName = res.FamilyName,
                        OrganisationName = message.LegalEntityName,
                        ApprenticeshipName = res.ApprenticeshipName,
                        Callback = new Uri(_nServiceBusOptions.CallbackUrl),
                        UserRedirect = new Uri(_nServiceBusOptions.RedirectUrl),
                    };

                    _logger.LogInformation($"SendInvitation {JsonConvert.SerializeObject(invite)}");

                    await context.Send(invite);
                }
            }
        }

        public Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
            => _api.UpdateApprenticeship(message.ToApprenticeshipUpdated());

        public Task Handle(UpdateEmailAddressCommand message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received {nameof(UpdateEmailAddressCommand)} for apprentice {message.ApprenticeId}");
            return _api.UpdateApprenticeEmail(message.ApprenticeId, message.ToEmailUpdate());
        }

        public async Task Handle(SendInvitationReply message, IMessageHandlerContext context)
            => await Task.CompletedTask;
    }
}