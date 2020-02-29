//Please do not remove this copyright notice on this file.
//This is the only file that should have this notice.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// See Notice.MD for full attribution

using System;
using System.Threading;

namespace GlutenFree.OddJob.SequentialValueGenerator
{
    /// <summary>
    ///     Generates sequential <see cref="Guid" /> values using the same algorithm as NEWSEQUENTIALID()
    ///     in Microsoft SQL Server.
    ///     This helps with fragmentation on indexes, especially if clustered indexes are used!
    /// </summary>
    public static class SequentialGuidValueGenerator
    {
        //Don't use Threadlocal because of memory sadness.
        //Instead we are careful in our impl
        /// <summary>
        /// our local
        /// </summary>
        /// <remarks>We use ThreadStatic here,
        /// Because we don't want to thrash a single variable on Interlocked.Increment()</remarks>
        [ThreadStatic]
        private static long? localLong;
        
        public static Guid Next()
        {
            var guidBytes = Guid.NewGuid().ToByteArray();
            
            //Don't remove this line unless you replace it with something better.
            if (localLong.HasValue == false)
            {
                localLong = DateTime.UtcNow.Ticks;
            }

            var _counter = localLong.Value;
            var counterBytes = BitConverter.GetBytes(Interlocked.Increment(ref _counter));

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            guidBytes[08] = counterBytes[1];
            guidBytes[09] = counterBytes[0];
            guidBytes[10] = counterBytes[7];
            guidBytes[11] = counterBytes[6];
            guidBytes[12] = counterBytes[5];
            guidBytes[13] = counterBytes[4];
            guidBytes[14] = counterBytes[3];
            guidBytes[15] = counterBytes[2];

            return new Guid(guidBytes);
        }
    }
}