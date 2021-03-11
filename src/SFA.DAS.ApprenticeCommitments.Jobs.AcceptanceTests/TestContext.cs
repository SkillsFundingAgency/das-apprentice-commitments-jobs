using SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests
{
    public class TestContext
    {
        public MockOuterApi Api { get; set; }
        public TestMessageBus TestMessageBus { get; set; }
        public DirectoryInfo WorkingDirectory { get; set; }
        public List<IHook> Hooks { get; } = new List<IHook>();
        internal FunctionsTestServer FunctionsServer { get; set; }
    }

    public interface IHook { }
}