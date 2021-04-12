using TechTalk.SpecFlow;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    [Binding]
    public class MockOuterApiBinding
    {
        public static MockOuterApi Client { get; set; }

        private readonly TestContext _context;

        public MockOuterApiBinding(TestContext context) => _context = context;

        [BeforeScenario(Order = 1)]
        public void InitialiseApi()
        {
            Client ??= new MockOuterApi();

            _context.Api = Client;
        }

        [AfterScenario()]
        public void Reset()
        {
            Client?.Reset();
        }

        [AfterFeature()]
        public static void CleanUpFeature()
        {
            Client?.Dispose();
            Client = null;
        }
    }
}