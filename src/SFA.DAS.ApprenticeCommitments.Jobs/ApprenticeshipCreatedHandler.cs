using Microsoft.Azure.WebJobs;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public interface IEcsApi
    {
        Task CreateApprentice(string email);
    }

    public static class ApprenticeshipCreatedHandler
    {
        [FunctionName("HandleApprenticeshipCreatedEvent")]
        public static async Task RunEvent(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.EmployerIncentives.AddEmployerVendorId")] ApprenticeshipCreatedEvent apprenticeshipCreated,
            IEcsApi api)
        {
            await api.CreateApprentice("bob");
        }
    }
}