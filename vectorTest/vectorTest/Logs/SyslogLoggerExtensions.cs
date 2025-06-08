using vectorTest.Logs;

namespace Microsoft.Extensions.Logging
{
    public static class SyslogLoggerExtensions
    {
        public static ILoggingBuilder AddSyslog(this ILoggingBuilder builder, string host, int port, string appName, Func<string, LogLevel, bool>? filter = null)
        {
            builder.Services.AddSingleton<ILoggerProvider>(sp => new SyslogLoggerProvider(host, port, appName, filter));
            return builder;
        }
    }
}
