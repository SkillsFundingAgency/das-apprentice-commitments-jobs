using RestEase;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public interface IEcsApi
    {
        [Post("apprenticeships")]
        Task<CreateApprenticeshipResponse> CreateApprentice([Body] ApprenticeshipCreated apprenticeship);

        [Post("apprenticeships/update")]
        Task UpdateApprenticeship([Body] ApprenticeshipUpdated apprenticeship);

        [Post("registrations/reminders")]
        Task SendInvitationReminders([Body] SendInvitationRemindersRequest request);

        [Get("registrations/reminders")]
        Task<RegistrationsWrapper> GetReminderRegistrations([Query] DateTime invitationCutOffTime);

        Task InvitationReminderSent(Guid apprenticeId, DateTime utcNow);

        [Post("apprentices/{id}/email")]
        Task UpdateApprenticeEmail([Path] Guid id, [Body] EmailUpdate request);
    }
}