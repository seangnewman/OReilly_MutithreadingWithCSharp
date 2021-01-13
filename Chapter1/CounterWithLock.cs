using System;

namespace Chapter1
{
    class CounterWithLock : CounterBase
    {
        private readonly object _syncRoot = new Object();

        public int Count { get; private set; }

        public override void Decrement()
        {
            lock (_syncRoot)
            {
                Count--;
            }
        }

        public override void Increment()
        {
            lock (_syncRoot)
            {
                Count++;
            }
        }
    }
}
