using SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services;
using System;
using System.Collections.Generic;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests
{
    public class TestContext : IDisposable
    {
        public MockOuterApi Api { get; set; }

        public TestMessageBus TestMessageBus { get; set; }

        public List<IHook> Hooks { get; } = new List<IHook>();
        internal FunctionsTestServer FunctionsServer { get; set; }

        public void Dispose()
        {
        }
    }

    public interface IHook { }
}