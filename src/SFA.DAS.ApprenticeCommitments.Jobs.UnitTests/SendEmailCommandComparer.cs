using SFA.DAS.Notifications.Messages.Commands;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    internal class SendEmailCommandComparer : IEqualityComparer<SendEmailCommand>
    {
        public bool Equals([AllowNull] SendEmailCommand x, [AllowNull] SendEmailCommand y)
        {
            if (x.TemplateId != y.TemplateId) return false;
            if (x.RecipientsAddress != y.RecipientsAddress) return false;
            foreach (var t in x.Tokens)
            {
                if (!x.Tokens.TryGetValue(t.Key, out var other)) return false;
                if (t.Value != other) return false;
            }
            return true;
        }

        public int GetHashCode([DisallowNull] SendEmailCommand obj) => obj.TemplateId.GetHashCode();
    }
}
