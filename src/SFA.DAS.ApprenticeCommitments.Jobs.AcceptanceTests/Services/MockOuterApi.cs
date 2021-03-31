using System;
using WireMock.Server;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    public class MockOuterApi : IDisposable
    {
        public WireMockServer MockServer { get; }
        public Uri BaseAddress { get; }

        public MockOuterApi()
        {
            MockServer = WireMockServer.Start(ssl: false);
            BaseAddress = new Uri(MockServer.Urls[0]);
        }

        public void Dispose()
        {
            MockServer?.Stop();
        }

        public void Reset()
        {
            MockServer.Reset();
        }
    }
}