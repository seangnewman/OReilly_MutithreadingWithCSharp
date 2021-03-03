using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter7_PLINQ
{
    public class Examples
    {
        static string EmulateProcessing(string taskName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(new Random(DateTime.Now.Millisecond).Next(250, 350)));

            Console.WriteLine($"{taskName} task was processed on a thread id  {Thread.CurrentThread.ManagedThreadId }");

            return taskName;
        }


        private void PrintInfo(string typeName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(150));
            Console.WriteLine($"{typeName} type was printed on a thread id {Thread.CurrentThread.ManagedThreadId}");
        }

        private static IEnumerable<string> GetTypes()
        {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                   from type in assembly.GetExportedTypes()
                   where type.Name.StartsWith("Web")
                   orderby type.Name.Length
                   select type.Name;
        }


        // ----------------------------------------------------------------------------
        public void UsingTheParallelClass()
        {

            // 1.  Invoke allows us to run several actions in Parallel
            Parallel.Invoke(
                () => EmulateProcessing("Task1"),
                () => EmulateProcessing("Task2"),
                () => EmulateProcessing("Task3")
                );

            var cts = new CancellationTokenSource();

            // 2. Use of Parallel.ForEach to loop through IEnumerable collections
            var result = Parallel.ForEach(
                Enumerable.Range(1, 30),

                new ParallelOptions { CancellationToken = cts.Token, MaxDegreeOfParallelism = Environment.ProcessorCount, TaskScheduler = TaskScheduler.Default },
                (i, state) =>
                   {
                       Console.WriteLine(i);
                       if (i == 20)
                       {
                           state.Break();
                           Console.WriteLine($"Loop is stopped: {state.IsStopped}");
                       }
                   });

            Console.WriteLine("--------");
            Console.WriteLine($"IsCompleted: {result.IsCompleted}");
            Console.WriteLine($"Lowest break iteration: {result.LowestBreakIteration}");
        }

        public void ParallelizingALINQQuery()
        {
            var sw = new Stopwatch();
            sw.Start();



            // Sequential LINQ method, everything runs on main thread
            var query = from t in GetTypes()
                        select EmulateProcessing(t);

            foreach (var typeName in query)
            {
                PrintInfo(typeName);
            }

            sw.Stop();
            Console.WriteLine("-----");
            Console.WriteLine("Sequential LINQ query ");
            Console.WriteLine($"Time elapsed: {sw.Elapsed}");
            Console.WriteLine("Press ENTER to continue .... ");
            Console.ReadLine();
            Console.Clear();
            sw.Reset();


            // Using Parallel execution where results are merged to single thread
            sw.Start();
            var parallelQuery = from t in GetTypes().AsParallel()
                                select EmulateProcessing(t);

            foreach (var typeName in parallelQuery)
            {
                PrintInfo(typeName);
            }

            sw.Stop();
            Console.WriteLine("-----");
            Console.WriteLine("Parallel LINQ query.  The results are being merged on a single thread");
            Console.WriteLine($"Time elapsed: {sw.Elapsed}");
            Console.WriteLine("Press ENTER to continue .... ");
            Console.ReadLine();
            Console.Clear();
            sw.Reset();

            // Running LINQ query in the same thread they were processed in (very fast)
            sw.Start();
            parallelQuery = from t in GetTypes().AsParallel()
                            select EmulateProcessing(t);
            parallelQuery.ForAll(PrintInfo);

            sw.Stop();
            Console.WriteLine("-----");
            Console.WriteLine("Parallel LINQ query.  The results are being processed in parallel");
            Console.WriteLine($"Time elapsed: {sw.Elapsed}");
            Console.WriteLine("Press ENTER to continue .... ");
            Console.ReadLine();
            Console.Clear();
            sw.Reset();

            // Tranform a PLINQ queryy to sequential
            sw.Start();
            query = from t in GetTypes().AsParallel().AsSequential()
                    select EmulateProcessing(t);

            foreach (var typeName in query)
            {
                PrintInfo(typeName);
            }

            sw.Stop();
            Console.WriteLine("-----");
            Console.WriteLine("Parallel LINQ query, transformed into seqential.");
            Console.WriteLine($"Time elapsed: {sw.Elapsed}");
            Console.WriteLine("Press ENTER to continue .... ");
            Console.ReadLine();
            Console.Clear();

        }

        public  void TweakingParametersOfAPLINQQuery()
        {
            var parallelQuery = from t in GetTypes().AsParallel()
                                select EmulateProcessing(t);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            try
            {
                parallelQuery.WithDegreeOfParallelism(Environment.ProcessorCount)
                                        .WithExecutionMode(ParallelExecutionMode.ForceParallelism)  // forces to run in parallel
                                        .WithMergeOptions(ParallelMergeOptions.Default)                     // Buffers results before returning
                                        .WithCancellation(cts.Token)                                                         // Alows yyou to cancel the quuery after 3 seconds
                                        .ForAll(Console.WriteLine);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("----------");
                Console.WriteLine("Operation has been cancelled");
                
            }

            Console.WriteLine("-------------");
            Console.WriteLine("Unordered PLINQ query execution");
            var unorderedQuery = from i in ParallelEnumerable.Range(1, 30)
                                 select i;
            foreach (var i in unorderedQuery)
            {
                Console.WriteLine(i);
            }

        }

    }
}
