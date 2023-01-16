using SFA.DAS.ApprenticeCommitments.Jobs.FakeServer;

OuterApiBuilder.Create(5121)
       .WithUpdateApproval()
       .Build();

Console.WriteLine("Press any key to stop the servers");
Console.ReadKey();