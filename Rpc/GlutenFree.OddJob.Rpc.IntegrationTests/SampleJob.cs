﻿using System;
using System.Collections.Concurrent;

namespace OddJob.Rpc.IntegrationTests
{
    public class SampleJob
    {
        public static bool fastLock = false; 
        public static ConcurrentDictionary<string, int> setter =
            new ConcurrentDictionary<string, int>();
        public void DoThing(string thing)
        {
            if (fastLock == false)
            {
                var val = setter.AddOrUpdate("lol", (l) => 1, (s, i) => i + 1);
                if (val < 2)
                {
                    Console.WriteLine(thing);
                }
                else
                {
                    fastLock = true;
                }
            }
        }
    }
}