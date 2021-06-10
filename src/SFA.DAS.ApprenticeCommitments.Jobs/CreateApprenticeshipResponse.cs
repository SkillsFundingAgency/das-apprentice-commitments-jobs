using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public class CreateApprenticeshipResponse
    {
        public string ClientId { get; set; }
        public Guid SourceId { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string ApprenticeshipName { get; set; }
        public string CallbackUrl { get; set; }
        public string RedirectUrl { get; set; }
    }
}
