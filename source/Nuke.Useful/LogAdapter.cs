using Microsoft.Extensions.Logging;
using Nuke.Common;
using System;

namespace Nuke.Useful
{
    internal class LogAdapter : ILogger
    {
        class Dummy : IDisposable
        {
            public void Dispose() { }
        }

        private static Dummy _dummy = new Dummy();

        public IDisposable BeginScope<TState>(TState state) => _dummy;

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var nukeLevel = logLevel switch
            {
                Microsoft.Extensions.Logging.LogLevel.Critical => Common.LogLevel.Error,
                Microsoft.Extensions.Logging.LogLevel.Error => Common.LogLevel.Error,
                Microsoft.Extensions.Logging.LogLevel.Information => Common.LogLevel.Normal,
                Microsoft.Extensions.Logging.LogLevel.None => Common.LogLevel.Normal,
                Microsoft.Extensions.Logging.LogLevel.Trace => Common.LogLevel.Trace,
                Microsoft.Extensions.Logging.LogLevel.Debug => Common.LogLevel.Trace,
                Microsoft.Extensions.Logging.LogLevel.Warning => Common.LogLevel.Warning,
                _ => throw new ArgumentException("Unknown log level: " + logLevel)
            };
            Logger.Log(nukeLevel, $"{eventId.Id} {formatter(state, exception)}");
        }
    }
}
