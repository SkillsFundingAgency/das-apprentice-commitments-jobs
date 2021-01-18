using RestEase;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public interface IEcsApi
    {
        [Post("apprenticeships")]
        Task CreateApprentice([Body] Apprenticeship email);
    }
}