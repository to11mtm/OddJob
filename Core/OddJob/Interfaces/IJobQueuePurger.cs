using System;

namespace GlutenFree.OddJob.Interfaces
{
    public interface IJobQueuePurger
    {
        void PurgeQueue(string name, string stateToPurge, DateTime purgeOlderThan);
    }
}