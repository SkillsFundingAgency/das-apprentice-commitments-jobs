using System;
using WireMock.Server;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Steps
{
    public class ApprenticeCommitmentsTestApi : IDisposable
    {
        public WireMockServer Server { get; }
        public Uri BaseAddress { get; }

        public ApprenticeCommitmentsTestApi()
        {
            Server = WireMockServer.Start(ssl: false);
            BaseAddress = new Uri(Server.Urls[0] + "/api");
        }

        public void Dispose()
        {
            /*if (Server?.IsStarted == true)*/ Server?.Stop();
            Server?.Dispose();
        }
    }
}