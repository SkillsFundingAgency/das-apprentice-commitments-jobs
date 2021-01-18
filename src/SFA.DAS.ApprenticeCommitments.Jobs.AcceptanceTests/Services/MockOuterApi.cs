using System;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    public class MockOuterApi : IDisposable
    {
        public WireMockServer Server { get; }
        public Uri BaseAddress { get; }

        public MockOuterApi()
        {
            Server = WireMockServer.Start(ssl: false);
            BaseAddress = new Uri(Server.Urls[0] + "/api");
        }

        internal void AcceptAllRequests()
        {
            Server
                .Given(Request.Create())
                .RespondWith(Response.Create().WithStatusCode(System.Net.HttpStatusCode.Accepted));
        }

        public void Dispose()
        {
            Server?.Stop();
            Server?.Dispose();
        }
    }
}