using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Handlers.CommitmentsEventHandlers
{
    [ExcludeFromCodeCoverage]
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
            var learningTypeString = GetLearningTypeString(message);

            if (!ShouldProcessLearningType(learningTypeString))
            {
                _logger.LogInformation(
                    "Ignoring ApprenticeshipCreatedEvent for {ApprenticeshipId} because LearningType = '{LearningType}'",
                    message.ApprenticeshipId,
                    learningTypeString);
                return;
            }

            _logger.LogInformation(
                "Handling ApprenticeshipCreatedEvent for {ApprenticeshipId} (ContinuationOfId = {ContinuationOfId})",
                message.ApprenticeshipId,
                message.ContinuationOfId);

            if (message.ContinuationOfId.HasValue)
                await _api.UpdateApproval(message.ToApprenticeshipUpdated());
            else
                await _api.CreateApproval(message.ToApprenticeshipCreated());
        }

        public async Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
        {
            var learningTypeString = GetLearningTypeString(message);

            if (!ShouldProcessLearningType(learningTypeString))
            {
                _logger.LogInformation(
                    "Ignoring ApprenticeshipUpdatedApprovedEvent for {ApprenticeshipId} because LearningType = '{LearningType}'",
                    message.ApprenticeshipId,
                    learningTypeString);
                return;
            }

            _logger.LogInformation(
                "Handling ApprenticeshipUpdatedApprovedEvent for {ApprenticeshipId}",
                message.ApprenticeshipId);

            await _api.UpdateApproval(message.ToApprenticeshipUpdated());
        }

        /// <summary>
        /// Returns true unless the LearningType is exactly "Apprenticeship Unit" (case‑insensitive).
        /// Null or empty values are processed (treated as not ApprenticeshipUnit).
        /// </summary>
        private static bool ShouldProcessLearningType(string? learningType)
        {
            if (string.IsNullOrWhiteSpace(learningType))
                return true; // No LearningType → process

            // Compare with the exact description used in the enum
            return !learningType.Equals("Apprenticeship Unit", StringComparison.OrdinalIgnoreCase)
                   && !learningType.Equals("ApprenticeshipUnit", StringComparison.OrdinalIgnoreCase); // safety net
        }

        /// <summary>
        /// Safely extracts the LearningType as a human‑readable string.
        /// Works even if the property is missing, null, or an enum.
        /// </summary>
        private static string? GetLearningTypeString(object message)
        {
            var prop = message.GetType().GetProperty("LearningType", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return null;

            var value = prop.GetValue(message);
            if (value == null)
                return null;

            // If it's already a string (unlikely but possible), return it directly
            if (value is string str)
                return str;

            // If it's the LearningType enum, get its Description attribute
            if (value is LearningType enumValue)
            {
                var memberInfo = typeof(LearningType).GetMember(enumValue.ToString());
                if (memberInfo.Length > 0)
                {
                    var descriptionAttr = memberInfo[0].GetCustomAttribute<DescriptionAttribute>();
                    if (descriptionAttr != null)
                        return descriptionAttr.Description; // "Apprenticeship Unit", etc.
                }
                return enumValue.ToString(); // Fallback to enum name ("ApprenticeshipUnit")
            }

            // Unexpected type – log and return null (process by default)
            return null;
        }
    }
}