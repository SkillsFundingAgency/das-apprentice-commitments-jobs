using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.InternalMessages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.EventHandlers.CommitmentsEventHandlers
{
    public class StoppedApprenticeshipSagaData : ContainSagaData
    {
        public long CommitmentsApprenticeshipId { get; set; }
        public DateTime CommitmentsStoppedOn { get; set; }
    }

    public class StoppedApprenticeshipTimeout { }

    public class StoppedApprenticeshipHandler
        : Saga<StoppedApprenticeshipSagaData>
        , IAmStartedByMessages<ApprenticeshipStoppedEvent>
        , IHandleMessages<ApprenticeshipCreatedEvent>
        , IHandleTimeouts<StoppedApprenticeshipTimeout>
    {
        private readonly ApplicationSettings _settings;
        private readonly ILogger<StoppedApprenticeshipHandler> _logger;

        public StoppedApprenticeshipHandler(ApplicationSettings settings, ILogger<StoppedApprenticeshipHandler> logger)
            => (_settings, _logger) = (settings, logger);

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<StoppedApprenticeshipSagaData> mapper)
        {
            mapper
                .ConfigureMapping<ApprenticeshipStoppedEvent>(message => message.ApprenticeshipId)
                .ToSaga(saga => saga.CommitmentsApprenticeshipId);
            mapper
                .ConfigureMapping<ApprenticeshipCreatedEvent>(message => message.ContinuationOfId ?? 0)
                .ToSaga(saga => saga.CommitmentsApprenticeshipId);
        }

        public async Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
        {
            Data.CommitmentsStoppedOn = message.StopDate;

            var delay = _settings.TimeToWaitBeforeStoppingApprenticeship;
            _logger.LogInformation("Deferring ApprenticeshipStoppedEvent for {commitmentsApprenticeshipId} until {delay}",
                message.ApprenticeshipId, DateTime.UtcNow.Add(delay));
            await RequestTimeout(context, delay, new StoppedApprenticeshipTimeout());
        }

        public Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Abandoning ApprenticeshipStoppedEvent for {commitmentsApprenticeshipId} due to continuation by {continuationApprenticeshipId}", message.ApprenticeshipId, message.ApprenticeshipId);
            MarkAsComplete();
            return Task.CompletedTask;
        }

        public async Task Timeout(StoppedApprenticeshipTimeout state, IMessageHandlerContext context)
        {
            _logger.LogInformation("Processing ApprenticeshipStoppedEvent for {commitmentsApprenticeshipId}", Data.CommitmentsApprenticeshipId);
            await context.SendLocal(new ProcessStoppedApprenticeship
            {
                CommitmentsApprenticeshipId = Data.CommitmentsApprenticeshipId,
                CommitmentsStoppedOn = Data.CommitmentsStoppedOn,
            });
        }
    }
}
