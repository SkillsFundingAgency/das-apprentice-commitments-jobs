using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RestEase.HttpClientFactory;
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
            //builder.Services.AddTransient<ApprenticeCommitmentsApi>();

            builder.Services.AddTransient<Http.MessageHandlers.DefaultHeadersHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.LoggingMessageHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.ApimHeadersHandler>();

            builder.Services.ConfigureOptions<ApprenticeCommitmentsApiOptions>(
                ApprenticeCommitmentsApiOptions.ApprenticeCommitmentsApi);

            var url = builder.Services
                .BuildServiceProvider()
                .GetRequiredService<IOptions<ApprenticeCommitmentsApiOptions>>()
                .Value.ApiBaseUrl;

            builder.Services.AddRestEaseClient<IEcsApi>(url)
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>()
                //.AddTypedClient<>
                ;
        }
    }
}