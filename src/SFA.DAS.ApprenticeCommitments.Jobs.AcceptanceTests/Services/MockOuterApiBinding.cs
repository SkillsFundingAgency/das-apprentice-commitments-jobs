using TechTalk.SpecFlow;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    [Binding]
    public class MockOuterApiBinding
    {
        private readonly TestContext context;

        public MockOuterApiBinding(TestContext context) => this.context = context;

        [BeforeScenario(Order = 2)]
        public void InitialiseApi()
        {
            context.Api = new MockOuterApi();
        }
    }
}