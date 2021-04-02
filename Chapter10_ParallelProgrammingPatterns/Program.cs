using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter10_ParallelProgrammingPatterns
{
    class Program
    {
        static void Main(string[] args)
        {
            Examples examples = new Examples();
            //examples.ImplementingLazyEvaluatedsharedsstates();
            //examples.ImplementingParallelPipelinewithBlockingCollection();
            //examples.ImplementingParallelPipelineWithTPLDataFlow();
            examples.ImplementingMapReduceWithPLINQ();
        }
    }
}
