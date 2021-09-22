using System;
using System.Threading.Tasks;
using RestEase;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Api
{
    public interface IEcsApi
    {
        [Post("registrations")]
        Task<CreateRegistrationResponse> CreateApprentice([Body] ApprenticeshipCreated apprenticeship);

        [Put("registrations")]
        Task UpdateApprenticeship([Body] ApprenticeshipUpdated apprenticeship);

        [Get("/apprentices/{apprenticeid}")]
        Task<Functions.Api.Apprentice> GetApprentice([Path] Guid apprenticeid);

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

        [Post("apprentices/{id}/email-confirmation")]
        Task SetEmailAddressConfirmed([Path] Guid id, [Body] ApprenticeshipEmailAddressConfirmation request);
    }
}