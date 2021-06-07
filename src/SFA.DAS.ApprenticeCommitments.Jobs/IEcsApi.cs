using System;
using RestEase;
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

        [Post("apprentices/{id}/email")]
        Task UpdateApprenticeEmail([Path] Guid id, [Body] EmailUpdate request);
    }
}