using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Handlers.CommitmentsEventHandlers
{
    public class CommitmentsEventHandler
        : IHandleMessages<ApprenticeshipCreatedEvent>
        , IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
    {
        private readonly IEcsApi _api;
        private readonly ILogger<CommitmentsEventHandler> _logger;

        public CommitmentsEventHandler(
            IEcsApi api,
            ILogger<CommitmentsEventHandler> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            if (!ShouldProcessTrainingType(message, out var trainingType))
            {
                _logger.LogInformation(
                    "Ignoring ApprenticeshipCreatedEvent for {ApprenticeshipId} due to TrainingType={TrainingType}",
                    message.ApprenticeshipId,
                    trainingType);

                return;
            }

            _logger.LogInformation(
                "Handling ApprenticeshipCreatedEvent for {ApprenticeshipId} (continuation {ContinuationOfId})",
                message.ApprenticeshipId,
                message.ContinuationOfId);

            if (message.ContinuationOfId.HasValue)
                await _api.UpdateApproval(message.ToApprenticeshipUpdated());
            else
                await _api.CreateApproval(message.ToApprenticeshipCreated());
        }

        public Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
        {
            if (!ShouldProcessTrainingType(message, out var trainingType))
            {
                _logger.LogInformation(
                    "Ignoring ApprenticeshipUpdatedApprovedEvent for {ApprenticeshipId} due to TrainingType={TrainingType}",
                    message.ApprenticeshipId,
                    trainingType);

                return Task.CompletedTask;
            }

            _logger.LogInformation(
                "Handling ApprenticeshipUpdatedApprovedEvent for {ApprenticeshipId}",
                message.ApprenticeshipId);

            return _api.UpdateApproval(message.ToApprenticeshipUpdated());
        }

        private static bool ShouldProcessTrainingType(object message, out string trainingType)
        {
            trainingType = string.Empty;

            var property = message.GetType()
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .FirstOrDefault(p => p.Name == "TrainingType" && p.PropertyType == typeof(string));

            if (property == null)
            {
                return true;
            }

            trainingType = property.GetValue(message)?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(trainingType))
            {
                return true;
            }

            var normalised = string.Concat(trainingType.Where(c => !char.IsWhiteSpace(c)));

            return normalised.Equals("Apprenticeship", StringComparison.OrdinalIgnoreCase)
                || normalised.Equals("Foundation", StringComparison.OrdinalIgnoreCase);
        }
    }
}
