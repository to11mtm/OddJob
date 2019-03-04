namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    public interface IService2Contract
    {
        void WriteCounter<T>(T param);
    }
}