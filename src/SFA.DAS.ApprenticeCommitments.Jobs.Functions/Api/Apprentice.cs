using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public class Apprentice
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}