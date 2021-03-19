using System;
using WireMock.Server;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    public class MockOuterApi : IDisposable
    {
        private bool _isDisposed;

        public WireMockServer MockServer { get; }
        public string BaseAddress { get; }

        public MockOuterApi()
        {
            MockServer = WireMockServer.Start(ssl: false);
            BaseAddress = MockServer.Urls[0];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            MockServer.Reset();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                if (MockServer.IsStarted)
                {
                    MockServer.Stop();
                }
                MockServer.Dispose();
            }

            _isDisposed = true;
        }
    }
}