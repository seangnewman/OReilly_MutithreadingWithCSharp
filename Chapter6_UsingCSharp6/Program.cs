using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter6_UsingCSharp6
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Examples ex = new Examples();
            // ex.UsingAwaitToGetAsyncTaskResults();
            // ex.UsingAwaitOperatorInLambdaExpression();
            // ex.UsingAwaitWithConsequentAsyncTasks();
            // ex.UsingAwaitForParallelTasks();
            //ex.HandlingExceptionsInAsyncOperations();
            // ex.AvoidingUseOfCapturedSyncThread();
            // ex.WorkingAroundAsyncVoid();
            ex.DesigningCustomAwaitableTypes();
        }
    }
}
