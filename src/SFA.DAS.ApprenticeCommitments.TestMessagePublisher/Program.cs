using Microsoft.Extensions.Configuration;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.NServiceBus.Extensions;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;

IConfiguration config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("local.settings.json", optional: true)
    .Build();

var connectionString = config["Values:NServiceBusConnectionString"];
if (connectionString is null)
    throw new NotSupportedException("NServiceBusConnection should contain ServiceBus connection string");


var endpointConfiguration = new EndpointConfiguration("SFA.DAS.ApprenticeCommitments");
endpointConfiguration.EnableInstallers();
endpointConfiguration.UseMessageConventions();
endpointConfiguration.UseNewtonsoftJsonSerializer();

endpointConfiguration.SendOnly();

var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
transport.AddRouting(routeSettings =>
{
    routeSettings.RouteToEndpoint(typeof(SFA.DAS.ApprenticeCommitments.Messages.Commands.SendApprenticeshipInvitationCommand), QueueNames.ApprenticeshipCommitmentsJobs);
});

transport.ConnectionString(connectionString);

var endpointInstance = await Endpoint.Start(endpointConfiguration)
    .ConfigureAwait(false);

while (true)
{
    Console.Clear();
    Console.WriteLine("To Please select an option...");

    Console.WriteLine("1.  Send nSendApprenticeshipInvitationCommand");
    Console.WriteLine("2.  Send RemindApprenticeCommand");
    Console.WriteLine("3.  Send ProcessStoppedApprenticeship");

    Console.WriteLine("4.  Publish ApprenticeshipCreatedEvent");
    Console.WriteLine("5.  Publish ApprenticeshipUpdatedApprovedEvent");
    Console.WriteLine("6.  Publish ApprenticeshipStoppedEvent");
    Console.WriteLine("7.  Publish ApprenticeEmailAddressChanged");
    Console.WriteLine("8.  Publish ApprenticeshipChangedEvent");
    Console.WriteLine("9.  Publish ApprenticeshipConfirmationConfirmedEvent");
    Console.WriteLine("10. Publish ApprenticeshipRegisteredEvent");

    Console.WriteLine("X. Exit");

    var choice = Console.ReadLine()?.ToLower();
    switch (choice)
    {
        case "1":
            await SendMessage(endpointInstance, new SFA.DAS.ApprenticeCommitments.Messages.Commands.SendApprenticeshipInvitationCommand { CommitmentsApprenticeshipId = 123, ResendOn = DateTime.Now.AddDays(7) });
            break;

        // Internal - fired by timer
        case "2":
            await SendMessage(endpointInstance, new SFA.DAS.ApprenticeCommitments.Jobs.Functions.InternalMessages.Commands.RemindApprenticeCommand { RegistrationId = Guid.NewGuid() });
            break;

        // Nothing looks to call this?
        case "3":
            await SendMessage(endpointInstance, new SFA.DAS.ApprenticeCommitments.Jobs.Functions.InternalMessages.Commands.ProcessStoppedApprenticeship
            {
                CommitmentsApprenticeshipId = 888,
                CommitmentsStoppedOn = DateTime.Now,
            });
            break;

        case "4":
            await PublishMessage(endpointInstance, new SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent { 
                                                            AccountId = 123, 
                                                            AccountLegalEntityId = 456,
                                                            AccountLegalEntityPublicHashedId = "ABC123",
                                                            AgreedOn = DateTime.Now,
                                                            ApprenticeshipId = 123,
                                                            StartDate = DateTime.Now.AddMonths(1),
                                                            EndDate = DateTime.Now.AddMonths(13),
                                                            ProviderId = 789
                                                            });
            break;

        case "5":  // Keeps repeating
            await PublishMessage(endpointInstance, new SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipUpdatedApprovedEvent {
                                                            ApprenticeshipId = 123,
                                                            StartDate = DateTime.Now.AddMonths(1),
                                                            EndDate = DateTime.Now.AddMonths(13)                                                            
                                                            });
            
            break;

        case "6":
            await PublishMessage(endpointInstance, new SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipStoppedEvent { 
                                                            ApprenticeshipId = 111,
                                                            AppliedOn = DateTime.Now,
                                                            StopDate = DateTime.Now.AddMonths(1),
                                                            });
            break;

        case "7":
            await PublishMessage(endpointInstance, new SFA.DAS.ApprenticeAccounts.Messages.Events.ApprenticeEmailAddressChanged
            {
                                                            ApprenticeId = Guid.NewGuid(),
                                                            ChangedOn = DateTime.Now
                                                            });
                break;

        case "8":
            await PublishMessage(endpointInstance, new ApprenticeshipChangedEvent {
                                                            ApprenticeId =  Guid.NewGuid(), 
                                                            ApprenticeshipId = 333, 
                                                            ConfirmationId = 444,
                                                            });
            break;

        case "9":
            await PublishMessage(endpointInstance, new ApprenticeshipConfirmationConfirmedEvent {
                                                            ConfirmationId = 123,
                                                            ApprenticeId = Guid.NewGuid(),
                                                            ApprenticeshipId = 456,
                                                            CommitmentsApprenticeshipId = 789,
                                                            });
            break;

        case "10":
            await PublishMessage(endpointInstance, new ApprenticeshipRegisteredEvent { RegistrationId = Guid.NewGuid() });
            break;

        case "x":
            await endpointInstance.Stop();
            return;
    }
}

async Task PublishMessage(IMessageSession messageSession, object message)
{
    await messageSession.Publish(message);

    Console.WriteLine("Message published.");
    Console.WriteLine("Press enter to continue");
    Console.ReadLine();
}

async Task SendMessage(IMessageSession messageSession, object message)
{
    await messageSession.Send(message);

    Console.WriteLine("Message sent.");
    Console.WriteLine("Press enter to continue");
    Console.ReadLine();
}