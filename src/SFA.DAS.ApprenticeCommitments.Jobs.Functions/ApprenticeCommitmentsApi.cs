using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using System.Threading.Tasks;
namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{

    public class ApprenticeCommitmentsApi : IEcsApi
    {
        public Task CreateApprentice(string email)
        {
            throw new System.NotImplementedException();
        }
    }
}