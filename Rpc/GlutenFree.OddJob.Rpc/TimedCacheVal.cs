using System;
using System.Threading;

namespace OddJob.RpcServer
{
    /// <summary>
    /// A Special Container for use with <see cref="LruTimedCache{T}"/>
    /// </summary>
    /// <typeparam name="TKey">The type of the Key</typeparam>
    internal class TimedCacheVal<TKey>
    {
        private TKey _key;
        public TKey Key
        {
            get
            {
                //When we grab the key, we increment here with Interlocked.
                //This way multiple connections will still use the same key.
                Interlocked.Increment(ref _lastUsed);
                return _key;
            }
            set
            {
                _key = value;
            }
        }

        private int _lastUsed;
        public int LastUsed
        {

            get
            {
                // We don't want long lived servers to get left out!
                // moduloing by 64 here ensures eventually
                // old servers catch up to new.
                // (Also, this means we never care about overflow :)
                return (_lastUsed & 0x40);
            }
        }
        public DateTime ExpiresAt { get; set; }
    }
}