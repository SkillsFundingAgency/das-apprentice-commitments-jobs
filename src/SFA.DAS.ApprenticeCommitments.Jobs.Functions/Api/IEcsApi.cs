﻿using RestEase;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public interface IEcsApi
    {
        [Post("apprenticeships")]
        Task CreateApprentice([Body] ApprenticeshipCreated apprenticeship);

        [Post("apprenticeships/update")]
        Task UpdateApprenticeship([Body] ApprenticeshipUpdated apprenticeship);

        [Get("/apprentices/{apprenticeid}")]
        Task<Apprentice> GetApprentice([Path] Guid apprenticeid);

        [Get("/apprentices/{apprenticeid}/apprenticeships/{apprenticeshipid}/revisions")]
        Task<ApprenticeshipHistory> GetApprenticeshipHistory([Path] Guid apprenticeid, [Path] long apprenticeshipid);

        [Post("registrations/reminders")]
        Task SendInvitationReminders([Body] SendInvitationRemindersRequest request);
    }
}