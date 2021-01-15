using SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services;
using System;
using System.Collections.Generic;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Steps
{
    public class TestContext : IDisposable
    {
        public ApprenticeCommitmentsTestApi Api { get; set; }

        public TestMessageBus TestMessageBus { get; set; }

        public List<IHook> Hooks { get; } = new List<IHook>();
        internal ApprenticeCommitmentApiTestServer FunctionsServer { get; set; }

        public void Dispose()
        {
        }
    }
}