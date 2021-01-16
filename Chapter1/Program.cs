using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter1
{
    class Program
    {
        static void Main(string[] args)
        {
            Examples example = new Examples();

            //example.CreateAThread();
            //example.MakingAThreadWait();
            //example.AbortAThread(); 
            //example.ThreadState();
            // example.ThreadPriority();
            //example.ForeGroundAndBackgroundThreads();
            // example.PassingThreadParameters();
            //example.ThreadUsingLocks();
            //example.ThreadLocksWithMonitors();
            example.ThreadExceptions();
            
        }
    }
}
