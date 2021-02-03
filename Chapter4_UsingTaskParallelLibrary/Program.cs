using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter4_UsingTaskParallelLibrary
{
    class Program
    {
        static void Main(string[] args)
        {
            var example = new Examples();
            //example.CreatingATask();
            //example.BasicOperations();
            //example.CombiningTasks();
            // example.ConvertingAPMToTasks();
            //example.ConvertingEAPToTasks();
            //example.CancellationOption();
            example.HandlingExceptions();

        }

       
    }
}
