using Microsoft.Azure.WebJobs;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public class HandleAddEmployerVendorIdCommand
    {
        public HandleAddEmployerVendorIdCommand()
        {
        }

        [FunctionName("HandleAddEmployerVendorIdCommand")]
        public Task RunEvent([NServiceBusTrigger(Endpoint = "SFA.DAS.EmployerIncentives.AddEmployerVendorId")] AddEmployerVendorIdCommand command)
        {
            return Task.CompletedTask;
        }
    }

    public class AddEmployerVendorIdCommand
    {
        public string HashedLegalEntityId { get; set; }
    }
}