using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter6_ConcurrentCollections
{
    class Examples
    {
        const string Item = "Dictionary item";
        const int Iterations = 1_000_000;
        public static string CurrentItem;
        static async Task RunProgram()
        {
            var cts = new CancellationTokenSource();
            #region using ConcurrentQueue
            //var taskQueue = new ConcurrentQueue<CustomTask>();


            //var taskSource = Task.Run(() => TaskProducer(taskQueue));

            //Task[] processors = new Task[4];     // Each task waits a random amount of time

            //for (int i = 1; i <= 4; i++)
            //{
            //    string processorId = i.ToString();

            //    // Added async/await to Task.  Method was completing before call was returning
            //    //processors[i - 1] = Task.Run( () => {
            //    //     TaskProcessor(taskQueue, $"Processor {processorId}", cts.Token);
            //    //});
            //    processors[i - 1] = Task.Run(async () =>
            //    {
            //        await TaskProcessor(taskQueue, $"Processor {processorId}", cts.Token);
            //    });

            //}
            #endregion
            #region using ConcurrentStack
            var taskStack = new ConcurrentStack<CustomTask>();

            var taskSource = Task.Run(() => TaskProducer(taskStack));

            Task[] processors = new Task[4];     // Each task waits a random amount of time

            for (int i = 1; i <= 4; i++)
            {
                string processorId = i.ToString();

                //  Added async/ await to Task.Method was completing before call was returning
                //processors[i - 1] = Task.Run(() =>
                //{
                //    TaskProcessor(taskQueue, $"Processor {processorId}", cts.Token);
                //});

                processors[i - 1] = Task.Run(async () =>
               {
                await TaskProcessor(taskStack, $"Processor {processorId}", cts.Token);
                });
            }
            #endregion


            await taskSource;
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            await Task.WhenAll(processors);
         }

        private static async Task TaskProcessor(ConcurrentQueue<CustomTask> queue, string name, CancellationToken token)
        {
            CustomTask workItem;
            bool dequeuSuccessful = false;

            await GetRandomDelay();

            do
            {
                dequeuSuccessful = queue.TryDequeue(out workItem);
                if (dequeuSuccessful)
                {
                    Console.WriteLine($"Task {workItem.Id} has been processed by {name}");
                }
                await GetRandomDelay();

            } while (!token.IsCancellationRequested);
        }
        private static async Task TaskProcessor(ConcurrentStack<CustomTask> stack, string name, CancellationToken token)
        {
            CustomTask workItem;
            bool dequeuSuccessful = false;

            await GetRandomDelay();

            do
            {
                dequeuSuccessful = stack.TryPop(out workItem);
                if (dequeuSuccessful)
                {
                    Console.WriteLine($"Task {workItem.Id} has been processed by {name}");
                }
                await GetRandomDelay();

            } while (!token.IsCancellationRequested);
        }
        private static Task GetRandomDelay()
        {
            int delay = new Random(DateTime.Now.Millisecond).Next(1, 500);
            return Task.Delay(delay);
        }

        private static async void TaskProducer(ConcurrentQueue<CustomTask>  queue)
        {
            for (int  i = 1;  i <= 20; i++)
            {
                await Task.Delay(50);
                var workItem = new CustomTask { Id = i };
                queue.Enqueue(workItem);
                Console.WriteLine($"Task  {workItem.Id} has been posted");
            }
        }
            private static async void TaskProducer(ConcurrentStack<CustomTask> stack)
            {
                for (int i = 1; i <= 20; i++)
                {
                    await Task.Delay(50);
                    var workItem = new CustomTask { Id = i };
                    stack.Push(workItem);
                    Console.WriteLine($"Task  {workItem.Id} has been posted");
                }
            }
            // -----------------------------------
            public void UsingConcurrentDictionary()
        {
            var concurrentDictionary = new ConcurrentDictionary<int, string>();
            var dictionary = new Dictionary<int, string>();

            var sw = new Stopwatch();

            sw.Start();
            for (int i = 0; i < Iterations; i++)
            {
                lock (dictionary)
                {
                    dictionary[i] = Item;
                }
            }
            sw.Stop();
            Console.WriteLine($"Writing to dictionary with a lock: {sw.Elapsed}");

            sw.Restart();
            for (int  i = 0;  i < Iterations; i++)
            {
                concurrentDictionary[i] = Item;
            }
            sw.Stop();
            Console.WriteLine($"Writing to a concurrent dictionary: {sw.Elapsed}");

            sw.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                lock (dictionary)
                {
                    CurrentItem = dictionary[i];
                }
            }
            sw.Stop();
            Console.WriteLine($"Reading from dictionary with lock: {sw.Elapsed}");

            sw.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                CurrentItem =concurrentDictionary[i];
            }
            sw.Stop();
            Console.WriteLine($"Reading from  a concurrent dictionary: {sw.Elapsed}");
        }

        public void ImplementingAsyncProcessingUsingConcurrentQueue()
        {
            Task t = RunProgram();
            t.Wait();

        }

        public void ImplementingAsyncProcessingUsingConcurrentStack()
        {
            Task t = RunProgram();
            t.Wait();

        }


    }
}
