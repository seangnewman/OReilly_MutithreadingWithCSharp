using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter3_UsingAThreadPool
{
    class Program
    {
        static void Main(string[] args)
        {
            Examples example = new Examples();

            //  example.InvokingADelegateOnThreadPool();
            //example.PostingOnAThreadPool();
            //example.ThreadPoolAndDegreeOfParallelism();
            example.ImplementingCancellationOption();
        }
    }
}
