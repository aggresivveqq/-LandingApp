using Serilog.Core;
using Serilog.Events;
using System.Threading.Channels;

namespace LandingApp.Logging
{
    public class ChannelLogSink : ILogEventSink
    {
        private readonly Channel<string> _channel;

        public ChannelLogSink(Channel<string> channel)
        {
            _channel = channel;
        }

        public void Emit(LogEvent logEvent)
        {
            var timestamp = logEvent.Timestamp.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            var level = logEvent.Level.ToString().ToUpper();
            var message = logEvent.RenderMessage();

            var logEntry = $"[{timestamp}] [{level}] {message}";

            if (logEvent.Exception != null)
            {
                logEntry += Environment.NewLine + logEvent.Exception;
            }

            if (logEvent.Properties.Count > 0)
            {
                var props = string.Join(", ", logEvent.Properties.Select(p => $"{p.Key}={p.Value}"));
                logEntry += Environment.NewLine + $"Properties: {props}";
            }

            if (!_channel.Writer.TryWrite(logEntry))
            {
                _ = _channel.Writer.WriteAsync(logEntry);
            }
        }
    }
}
