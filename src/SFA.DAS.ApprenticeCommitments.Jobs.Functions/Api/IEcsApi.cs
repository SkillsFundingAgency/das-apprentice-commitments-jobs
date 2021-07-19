using RestEase;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public interface IEcsApi
    {
        [Post("apprenticeships")]
        Task<CreateApprenticeshipResponse> CreateApprentice([Body] ApprenticeshipCreated apprenticeship);

        [Post("apprenticeships/update")]
        Task UpdateApprenticeship([Body] ApprenticeshipUpdated apprenticeship);

        [Get("/apprentices/{apprenticeid}")]
        Task<Api.Apprentice> GetApprentice([Path] Guid apprenticeid);

        [Get("/apprentices/{apprenticeid}/apprenticeships/{apprenticeshipid}/revisions")]
        Task<ApprenticeshipHistory> GetApprenticeshipHistory([Path] Guid apprenticeid, [Path] long apprenticeshipid);

        [Post("registrations/reminders")]
        Task SendInvitationReminders([Body] SendInvitationRemindersRequest request);

        [Get("registrations/reminders")]
        Task<RegistrationsWrapper> GetReminderRegistrations([Query] DateTime invitationCutOffTime);

        [Post("registrations/{apprenticeId}/reminder")]
        Task InvitationReminderSent([Path] Guid apprenticeId, [Body] RegistrationReminderSentRequest request);

        [Post("apprentices/{id}/email")]
        Task UpdateApprenticeEmail([Path] Guid id, [Body] EmailUpdate request);
    }
}