using System.Runtime.CompilerServices;

using StardewModdingAPI.Framework.Logging;

namespace UltimateStorageSystem.Utilities
{
    internal sealed class Logger : IMonitor
    {
        private static Logger   _instance = null!;
        private        IMonitor _monitor;

        /// <inheritdoc cref="IMonitor.IsVerbose"/>
        bool     IMonitor.IsVerbose => _instance._monitor.IsVerbose;
        /// <inheritdoc cref="IMonitor.Log"/>
        void IMonitor.Log(string message, LogLevel level) => _monitor.Log(message, level);

        /// <inheritdoc cref="IMonitor.LogOnce"/>
        void IMonitor.LogOnce(string message, LogLevel level) => _monitor.LogOnce(message, level);

        /// <inheritdoc cref="IMonitor.VerboseLog(string)"/>
        public void VerboseLog(string message) => _monitor.VerboseLog(message);

        /// <inheritdoc cref="IMonitor.VerboseLog(ref VerboseLogStringHandler)"/>
        public void VerboseLog([InterpolatedStringHandlerArgument("")] ref VerboseLogStringHandler message) => _monitor.VerboseLog(ref message);

        public Logger(IMonitor monitor)
        {
            _instance     = this;
            this._monitor = monitor;
        }

        /// <inheritdoc cref="IMonitor.Log"/>
        public static void Exception(Exception exception, string? message = null)
        {
            _instance._monitor.Log(exception.GetFullyQualifiedExceptionMessage(message), LogLevel.Error);
        }

        /// <inheritdoc cref="IMonitor.LogOnce"/>
        public static void ExceptionOnce(Exception exception, string? message = null)
        {
            _instance._monitor.LogOnce(exception.GetFullyQualifiedExceptionMessage(message), LogLevel.Error);
        }

        /// <inheritdoc cref="IMonitor.Log"/>
        public static void Error(string message)
        {
            _instance._monitor.Log(message, LogLevel.Error);
        }

        /// <inheritdoc cref="IMonitor.LogOnce"/>
        public static void ErrorOnce(string message)
        {
            _instance._monitor.LogOnce(message, LogLevel.Error);
        }

        /// <inheritdoc cref="IMonitor.Log"/>
        public static void Alert(string message)
        {
            _instance._monitor.Log(message, LogLevel.Alert);
        }

        /// <inheritdoc cref="IMonitor.LogOnce"/>
        public static void AlertOnce(string message)
        {
            _instance._monitor.LogOnce(message, LogLevel.Alert);
        }

        /// <inheritdoc cref="IMonitor.Log"/>
        public static void Debug(string message)
        {
            _instance._monitor.Log(message, LogLevel.Debug);
        }

        /// <inheritdoc cref="IMonitor.LogOnce"/>
        public static void DebugOnce(string message)
        {
            _instance._monitor.LogOnce(message, LogLevel.Debug);
        }

        /// <inheritdoc cref="IMonitor.Log"/>
        public static void Info(string message)
        {
            _instance._monitor.Log(message, LogLevel.Info);
        }

        /// <inheritdoc cref="IMonitor.LogOnce"/>
        public static void InfoOnce(string message)
        {
            _instance._monitor.LogOnce(message, LogLevel.Info);
        }

        /// <inheritdoc cref="IMonitor.Log"/>
        public static void Trace(string message)
        {
            _instance._monitor.Log(message, LogLevel.Trace);
        }

        /// <inheritdoc cref="IMonitor.LogOnce"/>
        public static void TraceOnce(string message)
        {
            _instance._monitor.LogOnce(message, LogLevel.Trace);
        }

        /// <inheritdoc cref="IMonitor.Log"/>
        public static void Warn(string message)
        {
            _instance._monitor.Log(message, LogLevel.Warn);
        }

        /// <inheritdoc cref="IMonitor.LogOnce"/>
        public static void WarnOnce(string message)
        {
            _instance._monitor.LogOnce(message, LogLevel.Warn);
        }

        /// <inheritdoc cref="IMonitor.VerboseLog(string)"/>
        public static void Verbose(string message)
        {
            _instance._monitor.VerboseLog(message);
        }

        /// <inheritdoc cref="IMonitor.VerboseLog(ref VerboseLogStringHandler)"/>
        public static void Verbose(ref VerboseLogStringHandler message)
        {
            _instance._monitor.VerboseLog(ref message);
        }

        /// <inheritdoc cref="IMonitor.IsVerbose"/>
        public static bool IsVerbose => _instance._monitor.IsVerbose;
    }
}
