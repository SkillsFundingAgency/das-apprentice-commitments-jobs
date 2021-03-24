using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Transport;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    internal class FunctionsTestServer : IDisposable
    {
        private bool _isDisposed;
        private readonly TestContext _context;
        private readonly Dictionary<string, string> hostConfig = new Dictionary<string, string>();

        private readonly Dictionary<string, string> appConfig;

        private IHost _host;

        public SendInvitationRemindersHandler SendInvitationRemindersHandler { get; private set; }

        public FunctionsTestServer(TestContext context)
        {
            _isDisposed = false;
            _context = context;
            appConfig = new Dictionary<string, string>
            {
                { "EnvironmentName", "LOCAL" },
                { "ConfigurationStorageConnectionString", "UseDevelopmentStorage=true" },
                { "ConfigNames", "SFA.DAS.EmployerIncentives.Functions" },
                { "NServiceBusConnectionString", "UseDevelopmentStorage=true" },
                { "AzureWebJobsStorage", "UseDevelopmentStorage=true" },
                { "SendRemindersAfterThisNumberDays", TestContext.SendRemindersAfterThisNumberDays.ToString() },
            };
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
                            a.ApiBaseUrl = _context.Api.BaseAddress;
                            a.SubscriptionKey = "";
                        });
                    })
                    .ConfigureWebJobs(startUp.Configure)
                ;

            hostBuilder.ConfigureServices((s) =>
            {
                s.AddNServiceBus<FunctionsTestServer>(o =>
                    {
                        o.EndpointConfiguration = (endpoint) =>
                        {
                            endpoint.UseTransport<LearningTransport>().StorageDirectory(_context.TestMessageBus.StorageDirectory.FullName);
                            return endpoint;
                        };

                        if (_context.Hooks.SingleOrDefault(h => h is MessageBusHook<MessageContext>) is MessageBusHook<MessageContext> hook)
                        {
                            o.OnMessageReceived += (message) => hook?.OnReceived?.Invoke(message);
                            o.OnMessageProcessed += (message) => hook?.OnProcessed?.Invoke(message);
                            o.OnMessageErrored += (exception, message) => hook?.OnErrored?.Invoke(exception, message);
                        }
                    });
            });

            hostBuilder.UseEnvironment("LOCAL");
            _host = await hostBuilder.StartAsync();

            SendInvitationRemindersHandler = new SendInvitationRemindersHandler(
                _host.Services.GetService(typeof(IEcsApi)) as IEcsApi, 
                _host.Services.GetService(typeof(IConfiguration)) as IConfiguration);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _host?.Dispose();
            }

            _isDisposed = true;
        }
    }
}