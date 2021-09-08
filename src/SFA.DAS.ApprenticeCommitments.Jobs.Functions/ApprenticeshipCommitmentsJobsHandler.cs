using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.Apprentice.LoginService.Messages;
using SFA.DAS.Apprentice.LoginService.Messages.Commands;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Notifications.Messages.Commands;
using System.Collections.Generic;
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
        private readonly ApplicationSettings _settings;

        public ApprenticeshipCommitmentsJobsHandler(
            ApplicationSettings settings,
            IEcsApi api,
            ILogger<ApprenticeshipCommitmentsJobsHandler> logger
                                                   )
        {
            _api = api;
            _logger = logger;
            _settings = settings;
        }

        public async Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            var templateId = "4f04cf81-b291-4577-9452-ecab875ed6f8";
            //if (!_settings.Notifications.Templates.TryGetValue("ApprenticeshipChangedEmail", out var templateId))
            //    throw new InvalidOperationException("Missing configuration `Notifications:Templates:ApprenticeshipChangedEmail`");

            if (message.ContinuationOfId.HasValue)
            {
                await _api.UpdateApprenticeship(message.ToApprenticeshipUpdated());
            }
            else
            {
                var res = await _api.CreateApprentice(message.ToApprenticeshipCreated());

                _logger.LogInformation($"_nServiceBusOptions {JsonConvert.SerializeObject(_settings)}");

                if (res != null)
                {
                    var link = $"{_settings.ApprenticeLoginApi.RedirectUrl}?Register={res.RegistrationId}";

                    var invite = new SendEmailCommand(templateId, res.Email,
                        new Dictionary<string, string>
                        {
                            { "GivenName", res.GivenName },
                            { "CreateAccountLink", link },
                            { "LoginLink", link },
                        });

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