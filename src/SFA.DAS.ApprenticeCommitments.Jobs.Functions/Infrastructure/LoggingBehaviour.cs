using Microsoft.Extensions.Logging;
using NServiceBus.Pipeline;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    internal class LogIncomingBehaviour
        : IBehavior<IIncomingLogicalMessageContext, IIncomingLogicalMessageContext>
    {
        private readonly ILogger _logger;

        public LogIncomingBehaviour()
        {
            _logger = LoggerFactory.Create(b => b.ConfigureLogging()).CreateLogger<LogIncomingBehaviour>();
        }

        public async Task Invoke(IIncomingLogicalMessageContext context, Func<IIncomingLogicalMessageContext, Task> next)
        {
            var intent = "fake";
            //context.MessageHeaders.TryGetValue("NServiceBus.MessageIntent", out var intent);
            var types = context.Message.MessageType.Name;
            _logger.LogInformation($"Received message {context.MessageId} (`{types}` intent `{intent}`)");

            await next(context);
        }
    }

    internal class LogOutgoingBehaviour
        : IBehavior<IOutgoingLogicalMessageContext, IOutgoingLogicalMessageContext>
    {
        private readonly ILogger _logger;

        public LogOutgoingBehaviour()
        {
            _logger = LoggerFactory.Create(b => b.ConfigureLogging()).CreateLogger<LogOutgoingBehaviour>();
        }

        public async Task Invoke(IOutgoingLogicalMessageContext context, Func<IOutgoingLogicalMessageContext, Task> next)
        {
            var types = context.Message.MessageType.Name;
            _logger.LogInformation($"Sending message {context.MessageId} (`{types}`)");

            await next(context);
        }
    }
}
