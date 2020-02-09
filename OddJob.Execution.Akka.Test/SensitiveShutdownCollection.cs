using Xunit;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    [CollectionDefinition("Require Synchronous Run", DisableParallelization = true)]
    public class SensitiveShutdownCollection : ICollectionFixture<ShutdownFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}