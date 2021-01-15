﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Transport;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    internal class FunctionsTestServer
    {
        private readonly TestContext context;
        private readonly Dictionary<string, string> hostConfig = new Dictionary<string, string>();

        private readonly Dictionary<string, string> appConfig = new Dictionary<string, string>
            {
                { "EnvironmentName", "LOCAL" },
                { "ConfigurationStorageConnectionString", "UseDevelopmentStorage=true" },
                { "ConfigNames", "SFA.DAS.EmployerIncentives.Functions" },
                { "NServiceBusConnectionString", "UseDevelopmentStorage=true" },
                { "AzureWebJobsStorage", "UseDevelopmentStorage=true" }
            };

        private IHost host;

        public FunctionsTestServer(TestContext context)
        {
            this.context = context;
        }

        public async Task Start()
        {
            var startUp = new Startup();

            var hostBuilder = new HostBuilder()
                    .ConfigureHostConfiguration(a =>
                    {
                        a.Sources.Clear();
                        a.AddInMemoryCollection(hostConfig);
                    })
                    .ConfigureAppConfiguration(a =>
                    {
                        a.Sources.Clear();
                        a.AddInMemoryCollection(appConfig);
                    })
                    .ConfigureWebJobs(c =>
                    {
                        c.Services.Configure<ApprenticeCommitmentsApiOptions>(a =>
                        {
                            a.ApiBaseUrl = context.Api.BaseAddress.ToString();
                            a.SubscriptionKey = "";
                        });
                    })
                    .ConfigureWebJobs(startUp.Configure)
                ;

            hostBuilder.ConfigureServices((s) =>
            {
                s.Configure<ApprenticeCommitmentsApiOptions>(a =>
                {
                    a.ApiBaseUrl = context.Api.BaseAddress.ToString();
                    a.SubscriptionKey = "";
                });

                s.AddNServiceBus(new LoggerFactory().CreateLogger<FunctionsTestServer>(),
                    o =>
                    {
                        o.EndpointConfiguration = (endpoint) =>
                        {
                            endpoint.UseTransport<LearningTransport>();
                            return endpoint;
                        };

                        if (context.Hooks.SingleOrDefault(h => h is MessageBusHook<MessageContext>) is MessageBusHook<MessageContext> hook)
                        {
                            o.OnMessageReceived += (message) => hook?.OnReceived?.Invoke(message);
                            o.OnMessageProcessed += (message) => hook?.OnProcessed?.Invoke(message);
                            o.OnMessageErrored += (exception, message) => hook?.OnErrored?.Invoke(exception, message);
                        }
                    });
            });

            hostBuilder.UseEnvironment("LOCAL");
            host = await hostBuilder.StartAsync();
        }
    }
}