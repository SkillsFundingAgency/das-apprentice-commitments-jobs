using System;

#nullable disable

namespace SFA.DAS.ApprenticeCommitments.Jobs
{
    public class CreateApprenticeshipResponse
    {
        public Guid SourceId { get; set; }
        public string Email { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string ApprenticeshipName { get; set; }
    }
}