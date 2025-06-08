using System.Net;
using System.Net.Sockets;
using System.Text;

namespace vectorTest.Logs;

public class SyslogLogger(string categoryName,
                        string host,
                        int port,
                        string applicationName,
                        Func<string, LogLevel, bool> filter) : ILogger
{
    private const int SyslogFacility = 16;

    public IDisposable BeginScope<TState>(TState state)
    {
        return NoopDisposable.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return (filter == null || filter(categoryName, logLevel));
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        var message = formatter(state, exception);

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        message = $"{logLevel}: {message}";

        if (exception != null)
        {
            message += Environment.NewLine + Environment.NewLine + exception.ToString();
        }

        var syslogLevel = MapToSyslogLevel(logLevel);
        Send(syslogLevel, message);
    }

    internal void Send(SyslogLogLevel logLevel, string message)
    {
        if (string.IsNullOrWhiteSpace(host) || port <= 0)
        {
            return;
        }

        var hostName = Dns.GetHostName();
        var timestamp = DateTime.Now.ToString("MMM dd HH:mm:ss"); // e.g., "Jun 07 13:45:00"
        var level = SyslogFacility * 8 + (int)logLevel;
        var logMessage = string.Format(
            "<{0}>{1} {2} {3}: {4}",
            level,
            timestamp,
            hostName,
            applicationName,
            message
        );
        Console.WriteLine(logMessage); // For debugging purposes
        var bytes = Encoding.UTF8.GetBytes(logMessage);

        using (var client = new UdpClient())
        {
            client.SendAsync(bytes, bytes.Length, host, port).Wait();
        }
    }

    private static SyslogLogLevel MapToSyslogLevel(LogLevel level)
    {
        if (level == LogLevel.Critical)
            return SyslogLogLevel.Critical;
        if (level == LogLevel.Debug)
            return SyslogLogLevel.Debug;
        if (level == LogLevel.Error)
            return SyslogLogLevel.Error;
        if (level == LogLevel.Information)
            return SyslogLogLevel.Info;
        if (level == LogLevel.None)
            return SyslogLogLevel.Info;
        if (level == LogLevel.Trace)
            return SyslogLogLevel.Info;
        if (level == LogLevel.Warning)
            return SyslogLogLevel.Warn;

        return SyslogLogLevel.Info;
    }
}
