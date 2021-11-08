using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.InternalMessages.Commands;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.DomainEvents
{
    public class RemindApprenticeCommandHandler : IHandleMessages<RemindApprenticeCommand>
    {
        private readonly IEcsApi _api;
        private readonly EmailService _emailer;

        public RemindApprenticeCommandHandler(
            IEcsApi api,
            EmailService emailer)
        {
            _api = api;
            _emailer = emailer;
        }

        public async Task Handle(RemindApprenticeCommand message, IMessageHandlerContext context)
        {
            var registration = await _api.GetRegistration(message.RegistrationId);

            await _emailer.SendApprenticeSignUpInvitation(context,
                registration.RegistrationId, registration.Email, registration.FirstName);

            await _api.InvitationReminderSent(
                message.RegistrationId,
                new RegistrationReminderSentRequest
                {
                    SentOn = DateTime.UtcNow
                });
        }
    }
}
