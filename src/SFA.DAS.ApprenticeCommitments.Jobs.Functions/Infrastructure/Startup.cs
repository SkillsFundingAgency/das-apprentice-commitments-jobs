using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;

[assembly: FunctionsStartup(typeof(SFA.DAS.ApprenticeCommitments.Jobs.Functions.Startup))]
namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.ConfigureLogging();
            builder.ConfigureConfiguration();
            builder.ConfigureNServiceBus();
            builder.Services.AddTransient<ApprenticeCommitmentsApi>();
        }
    }
}