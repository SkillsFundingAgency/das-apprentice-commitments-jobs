﻿using System;
using System.Collections.Generic;

#nullable disable

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public class Registration
    {
        public Guid ApprenticeId { get; set; }
        public long ApprenticeshipId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string EmployerName { get; set; }
        public string CourseName { get; set; }
    }

    public class RegistrationsWrapper
    {
        public IEnumerable<Registration> Registrations { get; set; }
    }
}