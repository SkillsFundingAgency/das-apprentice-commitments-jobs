using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;
using SFA.DAS.Apprentice.LoginService.Messages;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipCommitmentsJobsHandler
        : IHandleMessages<ApprenticeshipCreatedEvent>
        , IHandleMessages<SendInvitationReply>
        , IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
    {
        private readonly IEcsApi api;

        public ApprenticeshipCommitmentsJobsHandler(IEcsApi api) => this.api = api;

        public async Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            if (message.ContinuationOfId.HasValue)
            {
                await api.UpdateApprenticeship(message.ToApprenticeshipUpdated());
            }
            else
            {
                var res = await api.CreateApprentice(message.ToApprenticeshipCreated());

                var invite = new SendInvitation()
                {
                    ClientId = Guid.Parse(res.ClientId),
                    SourceId = res.SourceId.ToString(),                    
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

        public async Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
            => await api.UpdateApprenticeship(message.ToApprenticeshipUpdated());

        public async Task Handle(SendInvitationReply message, IMessageHandlerContext context)
        => await Task.CompletedTask;
    }
}