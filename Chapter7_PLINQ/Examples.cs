using System;
using System.Collections.Concurrent;
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
        //static string EmulateProcessing(string taskName)
        //{
        //    Thread.Sleep(TimeSpan.FromMilliseconds(new Random(DateTime.Now.Millisecond).Next(250, 350)));

        //    Console.WriteLine($"{taskName} task was processed on a thread id  {Thread.CurrentThread.ManagedThreadId }");

        //    return taskName;
        //}

        static string EmulateProcessing(string typeName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(150));

            Console.WriteLine($"{typeName} type  was processed on a thread id  {Thread.CurrentThread.ManagedThreadId }. Has { (typeName.Length % 2 == 0 ? "even" : "odd")} length");

            return typeName;
        }

        private void PrintInfo(string typeName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(150));
            Console.WriteLine($"{typeName} type was printed on a thread id {Thread.CurrentThread.ManagedThreadId}");
        }

        //private static IEnumerable<string> GetTypes()
        //{
        //    return from assembly in AppDomain.CurrentDomain.GetAssemblies()
        //           from type in assembly.GetExportedTypes()
        //           where type.Name.StartsWith("Web")
        //           orderby type.Name.Length
        //           select type.Name;
        //}

        private static IEnumerable<string> GetTypes()
        {
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetExportedTypes());

            return from type in types
                   where type.Name.StartsWith("Web")
                   select type.Name;
        }


        private static ConcurrentDictionary<char, int> MergeAccumulators(ConcurrentDictionary<char, int> total, ConcurrentDictionary<char, int> taskTotal)
        {
            foreach (var key in taskTotal.Keys)
            {
                if (total.ContainsKey(key))
                {
                    total[key] = total[key] + taskTotal[key];
                }
                else
                {
                    total[key] = taskTotal[key];
                }
            }
            Console.WriteLine("---------");
            Console.WriteLine($"Total aggregate value was calculated on a thread id {Thread.CurrentThread.ManagedThreadId}");
            return total;
        }

        private static ConcurrentDictionary<char, int> AccumulateLettersInformation(ConcurrentDictionary<char, int> taskTotal, string item)
        {
            foreach (var c in item)
            {
                if (taskTotal.ContainsKey(c))
                {
                    taskTotal[c] = taskTotal[c] + 1;
                }
                else
                {
                    taskTotal[c] = 1;
                }
            }
            Console.WriteLine($"{item} type was aggregated on a thread id {Thread.CurrentThread.ManagedThreadId}");
            return taskTotal;
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

        public void TweakingParametersOfAPLINQQuery()
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

        public void HandlingExceptionsInPLINQ()
        {
            IEnumerable<int> numbers = Enumerable.Range(-5, 10);

            var query = from number in numbers
                        select 100 / number;

            try
            {
                foreach (var n in query)
                {
                    Console.WriteLine(n);
                }
            }
            catch (DivideByZeroException)
            {

                Console.WriteLine("Divided by zero!");
            }

            Console.WriteLine("----------");
            Console.WriteLine("Sequential LINQ query processing");
            Console.WriteLine();

            var parallelQuery = from number in numbers.AsParallel()
                                select 100 / number;

            try
            {
                parallelQuery.ForAll(Console.WriteLine);
            }
            catch (DivideByZeroException)
            {
                Console.WriteLine("Divided by zero - usual exception handler!");
            }
            catch (AggregateException ex)
            {
                ex.Flatten().Handle(e =>
                {
                    if (e is DivideByZeroException)
                    {
                        Console.WriteLine("Divide by zero - aggregate exception handler!");
                        return true;
                    }
                    return false;
                });
            }

            Console.WriteLine("_______");
            Console.WriteLine("Parallel LINQ query processing and results merging");

        }

        public void ManagingDataPartioningPLINQQuery()
        {
            var timer = Stopwatch.StartNew();

            var partitioner = new StringPartitioner(GetTypes());

            var parallelQuery = from t in partitioner.AsParallel()
                                    //.WithDegreeOfParallelism(1)
                                select EmulateProcessing(t);

            parallelQuery.ForAll(PrintInfo);
            int count = parallelQuery.Count();
            timer.Stop();
            Console.WriteLine("------------------");
            Console.WriteLine($"Total items processed: {count}");
            Console.WriteLine($"Time elapsed: {timer.Elapsed}");
        }

        public void CreatingACustomAggregatorforPLINQ()
        {
            var parallelQuery = from t in GetTypes().AsParallel()
                                select t;

            var parallelAggregator = parallelQuery.Aggregate(
                                                                                                () => new ConcurrentDictionary<char, int>(),
                                                                                               (taskTotal, item) => AccumulateLettersInformation(taskTotal, item),
                                                                                               (total, taskTotal) => MergeAccumulators(total, taskTotal),
                                                                                               total => total);
            Console.WriteLine();
            Console.WriteLine("There were the following letters in type names");
            var orderedKeys = from k in parallelAggregator.Keys
                              orderby parallelAggregator[k] descending
                              select k;
            foreach (var c in orderedKeys)
            {
                Console.WriteLine($"Letter '{c}' ----- {parallelAggregator[c]} times");
            }


        }
    }
}
