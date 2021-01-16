using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter2_Synchronization
{
    class CounterLNoLock : CounterBase
    {
        // Using Interlocked makes it thread safe without use of locks... Interlocked uses atomic operations
        private int _count;
        public int Count => _count;

        public override void Decrement()
        {
            Interlocked.Decrement(ref _count);
        }

        public override void Increment()
        {
            Interlocked.Increment(ref _count);
        }
    }
}
