using System;
using System.Runtime.Serialization;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure
{
    [Serializable]
    public class MissingEmailTemplateConfigurationException : Exception
    {
        public MissingEmailTemplateConfigurationException(string templateName)
            : base(CreateMessage(templateName))
        {
        }

        public MissingEmailTemplateConfigurationException(string templateName, Exception? innerException)
            : base(CreateMessage(templateName), innerException)
        {
        }

        protected MissingEmailTemplateConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string CreateMessage(string templateName)
            => $"Missing configuration `Notifications:Templates:{templateName}`";
    }
}