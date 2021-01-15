using NServiceBus.Transport;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    [Binding]
    //[Scope(Tag = "messageBus")]
    public class TestMessageBusBinding
    {
        private readonly TestContext context;

        public TestMessageBusBinding(TestContext context) => this.context = context;

        [BeforeScenario(Order = 1)]
        public Task InitialiseMessageBus()
        {
            context.TestMessageBus = new TestMessageBus();
            context.Hooks.Add(new MessageBusHook<MessageContext>());
            return context.TestMessageBus.Start(/*_context.TestDirectory*/);
        }
    }
}