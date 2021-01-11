using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.IO;
using System.Reflection;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure
{
    internal static class EsfaLoggingExtension
    {
        internal static void ConfigureLogging(this IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(logBuilder =>
            {
                // all logging is filtered out by defualt
                logBuilder.AddFilter(typeof(Startup).Namespace, LogLevel.Information);
                var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
                var files = Directory.GetFiles(rootDirectory, "nlog.config", SearchOption.AllDirectories)[0];
                logBuilder.AddNLog(files);
            });
        }
    }
}