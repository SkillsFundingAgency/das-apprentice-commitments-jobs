using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using RestEase.HttpClientFactory;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

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

            var config = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

            builder.Services.AddTransient<Http.MessageHandlers.DefaultHeadersHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.LoggingMessageHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.ApimHeadersHandler>();

            builder.Services.AddOptions();
            builder.Services.Configure<ApprenticeCommitmentsApiOptions>(config.GetSection(ApprenticeCommitmentsApiOptions.ApprenticeCommitmentsApi));

            //serviceCollection.AddClient<IAgreementsService>((c, s) => new AgreementsService(c));

            builder.Services.AddRestEaseClient<IEcsApi>("https://google.com")
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>()
                //.AddTypedClient<>
                ;

            builder.Services.Configure(delegate (HttpClientFactoryOptions options)
            {
                options.HttpMessageHandlerBuilderActions.Add(delegate (HttpMessageHandlerBuilder b)
                {
                    var item = b.Services.GetRequiredService<Http.MessageHandlers.LoggingMessageHandler>();
                    b.AdditionalHandlers.Add(item);
                });
            });

        }
    }
}