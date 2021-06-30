using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;
using SFA.DAS.Apprentice.LoginService.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipCommitmentsJobsHandler
        : IHandleMessages<ApprenticeshipCreatedEvent>
        , IHandleMessages<SendInvitationReply>
        , IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
    {
        private readonly IEcsApi api;
        private readonly ILogger<ApprenticeshipCommitmentsJobsHandler> logger;

        public ApprenticeshipCommitmentsJobsHandler(IEcsApi api, ILogger<ApprenticeshipCommitmentsJobsHandler> logger)
        {
            this.api = api;
            this.logger = logger;
        }

        public async Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            if (message.ContinuationOfId.HasValue)
            {
                await api.UpdateApprenticeship(message.ToApprenticeshipUpdated());
            }
            else
            {
                var res = await api.CreateApprentice(message.ToApprenticeshipCreated());

                logger.LogInformation($"CreateApprentice returned {JsonConvert.SerializeObject(res)}");

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
                        Callback = res.CallbackUrl,
                        UserRedirect = res.RedirectUrl,
                    };

                    await context.Send(invite);
                }
            }
        }

        public async Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
            => await api.UpdateApprenticeship(message.ToApprenticeshipUpdated());

        public async Task Handle(SendInvitationReply message, IMessageHandlerContext context)
            => await Task.CompletedTask;
    }
}