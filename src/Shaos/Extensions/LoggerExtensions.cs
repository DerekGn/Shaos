namespace Shaos.Extensions
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(Level = LogLevel.Information,
            Message = "Event streaming started. ConnectionId: [{ConnectionId}]")]
        public static partial void EventStreamingStarted(this ILogger logger,
                                                         string connectionId);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "HttpContext is null")]
        public static partial void HttpContextNull(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "An unhandled exception occurred")]
        public static partial void LogUnhandledException(this ILogger logger,
                                                         Exception exception);
    }
}