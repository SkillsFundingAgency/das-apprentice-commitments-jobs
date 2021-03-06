using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.Apprentice.LoginService.Messages;
using SFA.DAS.Apprentice.LoginService.Messages.Commands;
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

        public ApprenticeshipCommitmentsJobsHandler(IEcsApi api, ILogger<ApprenticeshipCommitmentsJobsHandler> logger)
        {
            this._api = api;
            this._logger = logger;
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

                if (res != null)
                {
                    var invite = new SendInvitation()
                    {
                        ClientId = Guid.Parse(res.ClientId),
                        SourceId = res.SourceId.ToString(),
                        Email = res.Email,
                        GivenName = res.GivenName,
                        FamilyName = res.FamilyName,
                        OrganisationName = message.LegalEntityName,
                        ApprenticeshipName = res.ApprenticeshipName,
                        Callback = new Uri(res.CallbackUrl),
                        UserRedirect = new Uri(res.RedirectUrl),
                    };

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