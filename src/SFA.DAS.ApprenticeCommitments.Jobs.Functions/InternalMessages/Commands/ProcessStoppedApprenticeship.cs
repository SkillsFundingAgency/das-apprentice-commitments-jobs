using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.InternalMessages.Commands
{
    public class ProcessStoppedApprenticeship
    {
        public long CommitmentsApprenticeshipId { get; set; }
        public DateTime CommitmentsStoppedOn { get; set; }
    }
}