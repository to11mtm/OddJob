using System;

namespace OddJob.RpcServer
{
    public interface ITimedCache<T>
    {
        void Freshen(T item, DateTime expiresAt);
        T[] GetItems();
    }
}