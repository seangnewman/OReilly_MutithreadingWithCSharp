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
        static readonly Dictionary<string, string[]> _contentEmulation = new Dictionary<string, string[]>();



        //static async Task RunProgram()
        //{
        //    //var cts = new CancellationTokenSource();
        //    #region using ConcurrentQueue
        //    //var taskQueue = new ConcurrentQueue<CustomTask>();


        //    //var taskSource = Task.Run(() => TaskProducer(taskQueue));

        //    //Task[] processors = new Task[4];     // Each task waits a random amount of time

        //    //for (int i = 1; i <= 4; i++)
        //    //{
        //    //    string processorId = i.ToString();

        //    //    // Added async/await to Task.  Method was completing before call was returning
        //    //    //processors[i - 1] = Task.Run( () => {
        //    //    //     TaskProcessor(taskQueue, $"Processor {processorId}", cts.Token);
        //    //    //});
        //    //    processors[i - 1] = Task.Run(async () =>
        //    //    {
        //    //        await TaskProcessor(taskQueue, $"Processor {processorId}", cts.Token);
        //    //    });

        //    //}
        //    #endregion
        //    #region using ConcurrentStack
        //    //var taskStack = new ConcurrentStack<CustomTask>();

        //    //var taskSource = Task.Run(() => TaskProducer(taskStack));

        //    //Task[] processors = new Task[4];     // Each task waits a random amount of time

        //    //for (int i = 1; i <= 4; i++)
        //    //{
        //    //    string processorId = i.ToString();

        //    //    //  Added async/ await to Task.Method was completing before call was returning
        //    //    //processors[i - 1] = Task.Run(() =>
        //    //    //{
        //    //    //    TaskProcessor(taskQueue, $"Processor {processorId}", cts.Token);
        //    //    //});

        //    //    processors[i - 1] = Task.Run(async () =>
        //    //   {
        //    //    await TaskProcessor(taskStack, $"Processor {processorId}", cts.Token);
        //    //    });
        //    //}
        //    #endregion

        //    var bag = new ConcurrentBag<CrawlingTask>();
        //    string[] urls =  { "http://microsoft.com/", "http://google.com/", "http://facebook.com/", "http://twitter.com/" };

        //    var crawlers = new Task[4];
        //    for (int i = 1; i <= 4; i++)
        //    {
        //        string crawlerName = $"Crawler {i}";
        //        bag.Add(new CrawlingTask { UrlToCrawl = urls[i - 1], ProducerName = "root" });
        //        crawlers[i - 1] = Task.Run( () => Crawl(bag, crawlerName));
        //    }
         

        //    await Task.WhenAll(crawlers);
          
        // }

        static async Task RunProgram(IProducerConsumerCollection<CustomTask> collection = null)
        {
            var taskCollection = new BlockingCollection<CustomTask>();
            if (collection != null)
            {
                taskCollection = new BlockingCollection<CustomTask>(collection);
            }

            var taskSource = Task.Run( () => TaskProducer(taskCollection));

            Task[] processors = new Task[4];

            for (int i = 1; i <= 4; i++)
            {
                string processorId =  $"Processor {i}";
                processors[i - 1] = Task.Run( () => TaskProcessor(taskCollection, processorId));
            }

            await taskSource;
            await Task.WhenAll(processors);
        }

        private static async Task TaskProcessor(BlockingCollection<CustomTask> taskCollection, string processorId)
        {
            await GetRandomDelay();
            foreach (var item in taskCollection.GetConsumingEnumerable())
            {
                Console.WriteLine($"Task { item.Id} HashSet been processed by {processorId}");
                await GetRandomDelay();
            }
        }

        private static async Task TaskProducer(BlockingCollection<CustomTask> taskCollection)
        {
            for (int i = 1; i < 20; i++)
            {
                await Task.Delay(20);
                var workItem = new CustomTask { Id = i };
                taskCollection.Add(workItem);
                Console.WriteLine($"Task {workItem.Id} has been posted");
            }
            taskCollection.CompleteAdding();
        }

        private static async Task Crawl(ConcurrentBag<CrawlingTask> bag, string crawlerName)
        {
            CrawlingTask task;

            while (bag.TryTake(out task))
            {
                IEnumerable<string> urls = await GetLinksFromContent(task);
                if (urls != null)
                {
                    foreach (var url in urls)
                    {
                        var t = new CrawlingTask { UrlToCrawl = url, ProducerName = crawlerName };
                        bag.Add(t);
                    }
                }
                Console.WriteLine($"Indexing url {task.UrlToCrawl} posted by {task.ProducerName} is completed by {crawlerName}!");
            }
        }

        private static async Task<IEnumerable<string>> GetLinksFromContent(CrawlingTask task)
        {
            await GetRandomDelay();
            if (_contentEmulation.ContainsKey(task.UrlToCrawl) )
            {
                return _contentEmulation[task.UrlToCrawl];
            }

            return null;
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
        private void CreateLinks()
        {
            _contentEmulation["http://microsoft.com/"] = new[] { "http://microsoft.com/a.html", "http://microsoft.com/b.html" };
            _contentEmulation["http://microsoft.com/a.html"] = new[] { "http://microsoft.com/c.html", "http://microsoft.com/d.html" };
            _contentEmulation["http://microsoft.com/b.html"] = new[] { "http://microsoft.com/e.html" };

            _contentEmulation["http://google.com/"] = new[] { "http://google.com/a.html", "http://google.com/b.html" };
            _contentEmulation["http://google.com/a.html"] = new[] { "http://google.com/c.html", "http://google.com/d.html" };
            _contentEmulation["http://google.com/b.html"] = new[] { "http://google.com/e.html", "http://google.com/f.html" };
            _contentEmulation["http://google.com/c.html"] = new[] { "http://google.com/h.html", "http://google.com/i.html" };

            _contentEmulation["http://facebook.com/"] = new[] { "http://facebook.com/a.html", "http://facebook.com/b.html" };
            _contentEmulation["http://facebook.com/a.html"] = new[] { "http://facebook.com/c.html", "http://facebook.com/d.html" };
            _contentEmulation["http://facebook.com/b.html"] = new[] { "http://facebook.com/e.html" };

            _contentEmulation["http://twitter.com/"] = new[] { "http://twitter.com/a.html", "http://twitter.com/b.html" };
            _contentEmulation["http://twitter.com/a.html"] = new[] { "http://twitter.com/c.html", "http://twitter.com/d.html" };
            _contentEmulation["http://twitter.com/b.html"] = new[] { "http://twitter.com/e.html" };
            _contentEmulation["http://twitter.com/c.html"] = new[] { "http://twitter.com/f.html", "http://twitter.com/g.html" };
            _contentEmulation["http://twitter.com/d.html"] = new[] { "http://twitter.com/h.html" };
            _contentEmulation["http://twitter.com/e.html"] = new[] { "http://twitter.com/i.html" };

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

        public void CreateScalableCrawlerWithConcurrentBag()
        {
            CreateLinks();
            Task t = RunProgram();
            t.Wait();
        }

        public void AsynchronousProcessingWithBlockingCollection()
        {
            Console.WriteLine("Using a Queue inside of BlockingCollection");
            Console.WriteLine();

            
            Task t = RunProgram();
            t.Wait();

            Console.WriteLine();
            Console.WriteLine("Using a Stack inside a BlockingCollection");
            Console.WriteLine();

            t = RunProgram(new ConcurrentStack<CustomTask>());
            t.Wait();
        }
       
    }
}
