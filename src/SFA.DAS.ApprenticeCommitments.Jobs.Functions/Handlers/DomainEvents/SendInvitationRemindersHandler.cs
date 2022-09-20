//using Microsoft.Azure.WebJobs;
//using Microsoft.Extensions.Logging;
//using NServiceBus;
//using SFA.DAS.ApprenticeCommitments.Jobs.Api;
//using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
//using SFA.DAS.ApprenticeCommitments.Jobs.Functions.InternalMessages.Commands;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Handlers.DomainEvents
//{
//    public class SendInvitationRemindersHandler
//    {
//        private readonly IEcsApi _api;
//        private readonly IFunctionEndpoint _endpoint;
//        private readonly ApplicationSettings _options;

//        public SendInvitationRemindersHandler(
//            IEcsApi api,
//            IFunctionEndpoint endpoint,
//            ApplicationSettings options)
//        {
//            _api = api;
//            _endpoint = endpoint;
//            _options = options;
//        }

//        //[FunctionName("HandleSendInvitationReminders")]
//        public async Task Run(
//            [TimerTrigger("%SendRemindersTriggerTime%")] TimerInfo timer,
//            ExecutionContext executionContext,
//            ILogger log)
//        {
//            try
//            {
//                log.LogInformation("Getting Registrations which are due a reminder");

//                var reminder = await _api.GetReminderRegistrations(GetReminderCutoff());

//                await Task.WhenAll(reminder.Registrations.Select(SendReminder));

//                log.LogInformation("Send Invitation Reminders Job Complete");
//            }
//            catch (Exception e)
//            {
//                log.LogError(e, "Send Invitation Reminders Job has failed");
//            }

//            DateTime GetReminderCutoff() => DateTime.UtcNow.AddDays(-ReminderAfterDays);

//            async Task SendReminder(Registration registration)
//            {
//                var command = new RemindApprenticeCommand
//                {
//                    RegistrationId = registration.RegistrationId,
//                };

//                await _endpoint.Send(command, SendLocally.Options, executionContext, log);
//            }
//        }

//        internal long ReminderAfterDays
//        {
//            get
//            {
//                var remindAfterDays = Convert.ToInt32(_options.SendRemindersAfterThisNumberDays);
//                return remindAfterDays > 0 ? remindAfterDays : 7;
//            }
//        }
//    }
//}