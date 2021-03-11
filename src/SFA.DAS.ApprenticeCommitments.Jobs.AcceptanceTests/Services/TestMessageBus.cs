using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    public class TestMessageBus
    {
        private IEndpointInstance endpointInstance;
        public bool IsRunning { get; private set; }
        public DirectoryInfo StorageDirectory { get; private set; }

        public async Task Start(DirectoryInfo workingDirectory)
        {
            StorageDirectory = new DirectoryInfo(Path.Combine(workingDirectory.FullName, ".learningtransport"));
            if (!StorageDirectory.Exists)
            {
                Directory.CreateDirectory(StorageDirectory.FullName);
            }

            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerIncentives.Functions.Legalentities.TestMessageBus");
            endpointConfiguration
                .UseNewtonsoftJsonSerializer()
                .UseMessageConventions()
                .UseTransport<LearningTransport>()
                .StorageDirectory(StorageDirectory.FullName);
            endpointConfiguration.UseLearningTransport(s => s.RouteToEndpoint(typeof(ApprenticeshipCreated2Event), QueueNames.ApprenticeshipCreatedEvent));

            endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            IsRunning = true;
        }

        public async Task Stop()
        {
            await endpointInstance.Stop();
            IsRunning = false;
        }

        public Task Publish(object message) => endpointInstance.Publish(message);

        public Task Send(object message) => endpointInstance.Send(message);
    }

    public class MessageBusHook<T> : IHook
    {
        public Action<T> OnReceived { get; set; }
        public Action<T> OnProcessed { get; set; }
        public Action<Exception, T> OnErrored { get; set; }
    }
}