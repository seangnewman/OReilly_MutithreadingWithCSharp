using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter2_Synchronization
{
    class Counter : CounterBase
    {

        // Not thread safe
        private int _count;

        public int Count => _count;

        public override void Decrement()
        {
            _count--;
        }

        public override void Increment()
        {
            _count++;
        }
    }
}
