using NServiceBus;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions
{
    public static class SendLocally
    {
        public static SendOptions Options
        {
            get
            {
                var options = new SendOptions();
                options.RouteToThisEndpoint();
                return options;
            }
        }
    }
}
