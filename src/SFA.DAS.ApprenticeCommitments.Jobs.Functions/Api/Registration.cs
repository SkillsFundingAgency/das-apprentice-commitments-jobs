using System;
using System.Collections.Generic;

#nullable disable

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public class Registration
    {
        public Guid RegistrationId { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public Guid? ApprenticeId { get; set; }
    }

    public class RegistrationsWrapper
    {
        public IEnumerable<Registration> Registrations { get; set; }
    }
}