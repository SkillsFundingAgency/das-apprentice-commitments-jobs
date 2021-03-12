using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    [Binding]
    public class FunctionsTestServerBinding
    {
        private readonly TestContext _context;

        public FunctionsTestServerBinding(TestContext context) => _context = context;

        [BeforeScenario(Order = 3)]
        public async Task InitialiseFunctions()
        {
            _context.FunctionsServer = new FunctionsTestServer(_context);
            await _context.FunctionsServer.Start();
        }

        [AfterScenario()]
        public void CleanUp()
        {
            _context.FunctionsServer?.Dispose();
        }
    }
}