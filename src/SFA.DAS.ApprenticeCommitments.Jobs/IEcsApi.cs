using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public interface IEcsApi
    {
        Task CreateApprentice(string email);
    }
}