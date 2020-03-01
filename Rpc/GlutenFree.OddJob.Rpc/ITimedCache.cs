using System;

namespace OddJob.RpcServer
{
    public interface ITimedCache<T>
    {
        bool ShouldNotRandomize { get; }
        void Freshen(T item, DateTime expiresAt);
        T[] GetItems();
    }
}