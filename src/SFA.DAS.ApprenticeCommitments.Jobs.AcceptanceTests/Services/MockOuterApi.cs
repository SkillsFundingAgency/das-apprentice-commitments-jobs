using System;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
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
            BaseAddress = new Uri(MockServer.Urls[0] + "/api");
        }

        public void Dispose()
        {
            MockServer?.Stop();
            MockServer?.Dispose();
        }
    }
}