using MessagePack;

namespace OddJob.Rpc
{
    public class UnregisteredSerializerOptions : MessagePackSerializerOptions
    {
        public static UnregisteredSerializerOptions Instance { get; } = new UnregisteredSerializerOptions();
        protected UnregisteredSerializerOptions() : base(MessagePack.Resolvers.ContractlessStandardResolver.Instance)
        {
        }
    }
}