using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter2_Synchronization
{
    abstract class CounterBase
    {
        public abstract void Increment();
        public abstract void Decrement();
    }
}
