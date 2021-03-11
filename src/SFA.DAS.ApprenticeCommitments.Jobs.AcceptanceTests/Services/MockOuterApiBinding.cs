using System.Runtime.CompilerServices;
using TechTalk.SpecFlow;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    [Binding]
    public class MockOuterApiBinding
    {
        private readonly TestContext _context;

        public MockOuterApiBinding(TestContext context) => _context = context;

        [BeforeScenario(Order = 1)]
        public void InitialiseApi()
        {
            _context.Api = new MockOuterApi();
        }

        [AfterScenario()]
        public void Cleanup()
        {
            _context.Api?.MockServer.Reset();
            _context.Api?.Dispose();
        }
    }
}