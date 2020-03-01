using System;
using Akka.Dispatch;
using Akka.Event;

namespace GlutenFree.OddJob.Execution.Akka
{
    /// <summary>
    /// Uses the default Execution engine configuration (Console Logging of Akka Events and queue Timeouts.)
    /// </summary>
    public class StandardConsoleEngineLoggerConfig : IExecutionEngineLoggerConfig
    {
        public StandardConsoleEngineLoggerConfig(string logLevel)
        {
            LogLevel = logLevel;
        }
        /// <summary>
        /// Returns no logger, which results in the default console logger being used.
        /// </summary>
        public Func<IRequiresMessageQueue<ILoggerMessageQueueSemantics>> LoggerTypeFactory
        {
            get { return null; }
        }

        /// <summary>
        /// Defaults to INFO
        /// </summary>
        public string LogLevel { get; protected set; }
    }
}