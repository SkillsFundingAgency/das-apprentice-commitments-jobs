using RestEase;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public interface IEcsApi
    {
        [Post("registrations")]
        Task<CreateRegistrationResponse> CreateApprentice([Body] ApprenticeshipCreated apprenticeship);

        [Put("registrations")]
        Task UpdateApprenticeship([Body] ApprenticeshipUpdated apprenticeship);

        [Get("registrations/{id}")]
        Task<Registration> GetRegistration([Path] Guid id);

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

        [Patch("/apprentices/{apprenticeId}")]
        Task UpdateApprentice([Path] Guid apprenticeId, [Body] JsonPatchDocument<Apprentice> patch);
    }
}