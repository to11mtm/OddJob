using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Oddjob.Rpc.Redis.IntegrationTests
{
    public static class TaskExts
    {
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable ForAwait(this Task task) => task.ConfigureAwait(false);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredValueTaskAwaitable ForAwait(this in ValueTask task) => task.ConfigureAwait(false);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable<T> ForAwait<T>(this Task<T> task) => task.ConfigureAwait(false);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredValueTaskAwaitable<T> ForAwait<T>(this in ValueTask<T> task) => task.ConfigureAwait(false);

    }
}