namespace vectorTest.Logs;

public class SyslogLoggerProvider : ISyslogLoggerProvider
{
    private string _host;
    private int _port;
    private string _applicationName;
    private readonly Func<string, LogLevel, bool> _filter;

    public SyslogLoggerProvider(string host, int port, string applicationName, Func<string, LogLevel, bool> filter)
    {
        _host = host;
        _port = port;
        _applicationName = applicationName;
        _filter = filter;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SyslogLogger(categoryName, _host, _port, _applicationName, _filter);
    }

    public void Dispose()
    {
    }
}