using log4net;
using System;

namespace Osu.Mvvm.Miscellaneous
{
    /// <summary>
    /// Represents a logger
    /// </summary>
    public static class Log
    {
        #region Attributes
        /// <summary>
        /// The logger used
        /// </summary>
        private static ILog log = LogManager.GetLogger("osu!ui");
        #endregion

        #region Public Methods
        /// <summary>
        /// Displays an info message on the logger
        /// </summary>
        /// <param name="message">the message</param>
        public static void Info(object message)
        {
            log.Info(message);
        }

        /// <summary>
        /// Displays an info message on the logger with an exception
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="exception">the exception</param>
        public static void Info(object message, Exception exception)
        {
            log.Info(message, exception);
        }

        /// <summary>
        /// Displays a warn message on the logger
        /// </summary>
        /// <param name="message">the message</param>
        public static void Warn(object message)
        {
            log.Warn(message);
        }

        /// <summary>
        /// Displays a warn message on the logger with an exception
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="exception">the exception</param>
        public static void Warn(object message, Exception exception)
        {
            log.Warn(message, exception);
        }

        /// <summary>
        /// Displays an error message on the logger
        /// </summary>
        /// <param name="message">the message</param>
        public static void Error(object message)
        {
            log.Error(message);
        }

        /// <summary>
        /// Displays an error message on the logger with an exception
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="exception">the exception</param>
        public static void Error(object message, Exception exception)
        {
            log.Error(message, exception);
        }

        /// <summary>
        /// Displays a fatal message on the logger
        /// </summary>
        /// <param name="message">the message</param>
        public static void Fatal(object message)
        {
            log.Fatal(message);
        }

        /// <summary>
        /// Displays a fatal message on the logger with an exception
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="exception">the exception</param>
        public static void Fatal(object message, Exception exception)
        {
            log.Fatal(message, exception);
        }
        #endregion
    }
}
