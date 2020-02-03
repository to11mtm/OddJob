using Akka.Actor;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class SetRecordingConfiguration
    {
        public int NumRecorders { get; set; }
        public Props RecorderProps { get; set; }
    }
}