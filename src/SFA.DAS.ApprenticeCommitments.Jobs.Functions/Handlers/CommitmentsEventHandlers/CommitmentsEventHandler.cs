using System;
using System.Reflection;
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
            var learningType = GetLearningType(message);

            if (!ShouldProcessLearningType(learningType))
            {
                _logger.LogInformation(
                    "Ignoring ApprenticeshipCreatedEvent for {ApprenticeshipId} due to LearningType={LearningType}",
                    message.ApprenticeshipId,
                    learningType);

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
            var learningType = GetLearningType(message);

            if (!ShouldProcessLearningType(learningType))
            {
                _logger.LogInformation(
                    "Ignoring ApprenticeshipUpdatedApprovedEvent for {ApprenticeshipId} due to LearningType={LearningType}",
                    message.ApprenticeshipId,
                    learningType);

                return Task.CompletedTask;
            }

            _logger.LogInformation(
                "Handling ApprenticeshipUpdatedApprovedEvent for {ApprenticeshipId}",
                message.ApprenticeshipId);

            return _api.UpdateApproval(message.ToApprenticeshipUpdated());
        }

        private static bool ShouldProcessLearningType(string? learningType)
        {
            if (string.IsNullOrWhiteSpace(learningType))
            {
                return true;
            }

            return learningType.Equals("Apprenticeship", StringComparison.OrdinalIgnoreCase)
                || learningType.Equals("FoundationApprenticeship", StringComparison.OrdinalIgnoreCase)
                || learningType.Equals("Foundation Apprenticeship", StringComparison.OrdinalIgnoreCase);
        }

        private static string? GetLearningType(object message)
        {
            var prop = message.GetType().GetProperty("LearningType", BindingFlags.Public | BindingFlags.Instance);

            if (prop == null || prop.PropertyType != typeof(string))
            {
                return null;
            }

            return prop.GetValue(message) as string;
        }
    }
}
