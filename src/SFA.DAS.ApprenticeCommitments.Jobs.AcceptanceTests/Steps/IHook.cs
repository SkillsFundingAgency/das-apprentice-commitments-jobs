using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Steps
{
    public interface IHook
    {
    }

    public class Hook<T> : IHook
    {
        public Action<T> OnReceived { get; set; }
        public Action<T> OnProcessed { get; set; }
        public Action<Exception, T> OnErrored { get; set; }
    }
}