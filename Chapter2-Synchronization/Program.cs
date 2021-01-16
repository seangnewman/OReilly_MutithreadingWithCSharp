using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter2_Synchronization
{
    class Program
    {
        static void Main(string[] args)
        {
            var example = new Examples();

            //example.BasicAtomicOperations();
            // example.MutexConstruct();
            //example.SemaphoreSlimConstruct();
            // example.AutoResetEvent();
            example.ManualResetEventSlim();
        }
    }
}
