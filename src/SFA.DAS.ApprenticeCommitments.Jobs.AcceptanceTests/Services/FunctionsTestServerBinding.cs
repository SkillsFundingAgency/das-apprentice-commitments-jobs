using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    [Binding]
    public class FunctionsTestServerBinding
    {
        private readonly TestContext context;

        public FunctionsTestServerBinding(TestContext context) => this.context = context;

        [BeforeScenario(Order = 3)]
        public async Task InitialiseFunctions()
        {
            context.FunctionsServer = new FunctionsTestServer(context);
            await context.FunctionsServer.Start();
        }
    }
}