using RestEase;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public interface IEcsApi
    {
        [Post("apprenticeships")]
        Task CreateApprentice([Body] ApprenticeshipCreated apprenticeship);

        [Post("apprenticeships/update")]
        Task UpdateApprenticeship([Body] ApprenticeshipUpdated apprenticeship);

        [Post("registrations/reminders")]
        Task SendInvitationReminders([Body] SendInvitationRemindersRequest request);
    }
}