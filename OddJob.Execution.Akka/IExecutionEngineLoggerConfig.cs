using System;
using Akka.Dispatch;
using Akka.Event;

namespace GlutenFree.OddJob.Execution.Akka
{
    /// <summary>
    /// Interface defining a logger for the execution engine.
    /// <see cref="StandardConsoleEngineLoggerConfig"/> Is considered a safe default
    /// </summary>
    public interface IExecutionEngineLoggerConfig
    {
        /// <summary>
        /// This corresponds to a Type defined in an Akka.NET Logging plugin.
        /// </summary>
        Func<IRequiresMessageQueue<ILoggerMessageQueueSemantics>> LoggerTypeFactory { get; }
        /// <summary>
        /// This is expected to be a value of OFF, ERROR, WARNING, INFO, or DEBUG
        /// </summary>
        string LogLevel { get; }
    }
}