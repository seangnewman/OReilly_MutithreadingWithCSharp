using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter7_PLINQ
{
    class Program
    {
        static void Main(string[] args)
        {
            Examples examples = new Examples();

            //examples.UsingTheParallelClass();
            //examples.ParallelizingALINQQuery();
            //examples.TweakingParametersOfAPLINQQuery();
            //examples.HandlingExceptionsInPLINQ();
            //examples.ManagingDataPartioningPLINQQuery();
            examples.CreatingACustomAggregatorforPLINQ();
        }
    }
}
