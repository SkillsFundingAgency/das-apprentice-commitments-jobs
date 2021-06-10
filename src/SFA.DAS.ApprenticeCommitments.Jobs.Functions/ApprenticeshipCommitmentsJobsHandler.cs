using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;
using ls = SFA.DAS.Apprentice.LoginService.Messages;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public class ApprenticeshipCommitmentsJobsHandler
        : IHandleMessages<ApprenticeshipCreatedEvent>
        , IHandleMessages<ls.SendInvitationReply>
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
                // create registration
                var res = await api.CreateApprentice(message.ToApprenticeshipCreated());

                var invite = new ls.SendInvitation()
                {
                    ClientId = Guid.Parse(res.ClientId),
                    SourceId = res.SourceId.ToString(),                    
                    GivenName = res.GivenName,
                    FamilyName = res.FamilyName,
                    Email = "mrchas@chastest.com",
                    OrganisationName = message.LegalEntityName,
                    ApprenticeshipName = res.ApprenticeshipName,
                    Callback = res.CallbackUrl,
                    UserRedirect = res.RedirectUrl,
                };

                // create invitation
                await context.Send(invite);
            }
        }

        public async Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
            => await api.UpdateApprenticeship(message.ToApprenticeshipUpdated());

        public async Task Handle(ls.SendInvitationReply message, IMessageHandlerContext context)
        => await Task.CompletedTask;
    }
}