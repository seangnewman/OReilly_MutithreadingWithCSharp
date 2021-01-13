using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter1
{
    class Counter : CounterBase
    {
        public int Count { get; private set; }

        public override void Decrement()
        {
            Count--;
        }

        public override void Increment()
        {
            Count++;
        }
    }
}
