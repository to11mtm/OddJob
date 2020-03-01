using MessagePack;

namespace OddJob.Rpc
{
    public class UnregisteredSerializerOptions
    {
        public static MessagePackSerializerOptions Instance { get; } =
            MessagePack.MessagePackSerializer.DefaultOptions.WithResolver(
                MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        
    }
}