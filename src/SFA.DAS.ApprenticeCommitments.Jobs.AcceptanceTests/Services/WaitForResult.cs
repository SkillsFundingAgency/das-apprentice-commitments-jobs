using FluentAssertions;
using SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Steps;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.AcceptanceTests.Services
{
    public static class TestContextWaitForExtension
    {
        public class WaitForResult
        {
            public bool HasTimedOut { get; set; }
            public bool HasStarted { get; set; }
            public bool HasErrored { get; private set; }
            public Exception LastException { get; private set; }
            public bool HasCompleted { get; set; }

            public void SetHasErrored(Exception ex)
            {
                HasErrored = true;
                LastException = ex;
            }
        }

        public static async Task<WaitForResult> WaitFor<T>(
                   this TestContext context,
                   Func<Task> func,
                   bool assertOnTimeout = true,
                   bool assertOnError = false,
                   int timeoutInMs = 15000)
        {
            var waitForResult = new WaitForResult();

            var hook = context.Hooks.Find(h => h is Hook<T>) as Hook<T>;
            //var hook = context.Hooks.SingleOrDefault(h => h is Hook<T>) as Hook<T>;

            hook.OnReceived = _ => waitForResult.HasStarted = true;
            hook.OnProcessed = _ => waitForResult.HasCompleted = true;
            hook.OnErrored = (ex, _) => waitForResult.SetHasErrored(ex);

            try
            {
                await func();
            }
            catch (Exception ex)
            {
                waitForResult.SetHasErrored(ex);
            }

            await WaitForHandlerCompletion(waitForResult, timeoutInMs);

            if (assertOnTimeout)
                waitForResult.HasTimedOut.Should().Be(false, "handler should not have timed out");

            if (assertOnError)
                waitForResult.HasErrored.Should().Be(false, $"handler should not have errored with error '{waitForResult.LastException?.Message}'");

            return waitForResult;
        }

        private static async Task WaitForHandlerCompletion(WaitForResult waitForResult, int timeoutInMs)
        {
            using var timer = new Timer(new TimerCallback(TimedOutCallback), waitForResult, timeoutInMs, Timeout.Infinite);
            while (!waitForResult.HasCompleted && !waitForResult.HasTimedOut)
                await Task.Delay(100);
        }

        private static void TimedOutCallback(object state) =>
            ((WaitForResult)state).HasTimedOut = true;
    }
}