using System;

namespace GlutenFree.OddJob.Serializable
{
    /// <summary>
    /// A Job Definition in a Serializable format.
    /// Type information is stored in Assembly qualified Name format.
    /// Parameter Data is stored as JSON.
    /// Other data is stored in standard .NET types (GUID,string, etc.)
    /// </summary>
    public class SerializableOddJob
    {
        

        public SerializableOddJob()
        {
            

        }
        public Guid JobId { get; set; }
        public OddJobSerializedParameter[] JobArgs { get; set; }
        public string TypeExecutedOn { get; set; }
        public string MethodName { get; set; }
        public string Status { get; set; }
        public string[] MethodGenericTypes { get; set; }
        public RetryParameters RetryParameters { get; set; }
        public DateTimeOffset? ExecutionTime { get; set; }
        public string QueueName { get; set; }
    }
}