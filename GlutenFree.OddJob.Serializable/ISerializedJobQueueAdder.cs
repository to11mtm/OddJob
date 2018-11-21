namespace GlutenFree.OddJob.Serializable
{
    public interface ISerializedJobQueueAdder
    {
        void AddJob(SerializableOddJob jobData);
    }
}