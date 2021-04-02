using System.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Net.Http;

namespace Chapter10_ParallelProgrammingPatterns
{
    class Examples
    {
        private const int CollectionsNumber = 4;
        public const int Count = 5;
        static char[] delimiters = { ' ', ',', ';', ':', '\'', '.' };

        static void CreateInitialValues(BlockingCollection<int>[] sourceArrays, CancellationToken cts)
        {
            Parallel.For(0, sourceArrays.Length * Count, (j, state) => { });
        }



        //private static async Task ProcessAsynchronously()
        //{
        //    var unsafeState = new UnsafeState();
        //    Task[] tasks = new Task[4];

        //    for (int i = 0; i < 4; i++)
        //    {
        //        tasks[i] = Task.Run(() => Worker(unsafeState));
        //    }
        //    await Task.WhenAll(tasks);
        //    Console.WriteLine(" --------------------- ");

        //    var firstState = new DoubleCheckedLocking();

        //    for (int i = 0; i < 4; i++)
        //    {
        //        tasks[i] = Task.Run(() => Worker(firstState));
        //    }
        //    await Task.WhenAll(tasks);
        //    Console.WriteLine(" --------------------- ");

        //    var secondState = new BCLDoubleChecked();
        //    for (int i = 0; i < 4; i++)
        //    {
        //        tasks[i] = Task.Run(() => Worker(secondState));
        //    }
        //    await Task.WhenAll(tasks);
        //    Console.WriteLine(" --------------------- ");

        //    var lazy = new Lazy<ValueToAccess>(ValueToAccess.Compute);
        //    var thirdState = new LazyWrapper(lazy);
        //    for (int i = 0; i < 4; i++)
        //    {
        //        tasks[i] = Task.Run(() => Worker(secondState));
        //    }
        //    await Task.WhenAll(tasks);
        //    Console.WriteLine(" --------------------- ");

        //    var fourthState = new BCLThreadSafeFactory();
        //    for (int i = 0; i < 4; i++)
        //    {
        //        tasks[i] = Task.Run(() => Worker(secondState));
        //    }
        //    await Task.WhenAll(tasks);
        //    Console.WriteLine(" --------------------- ");

        //}

        //public static ValueToAccess Compute()
        //{
        //    Console.WriteLine($"The valie is being constructed on a thread id { Thread.CurrentThread.ManagedThreadId}");
        //    Thread.Sleep(TimeSpan.FromSeconds(1));

        //    return new ValueToAccess($"Constructed on thread id {Thread.CurrentThread.ManagedThreadId}");

        //}

        async static Task ProcessAsynchronously()
        {

            // Note: TPLDataFlow library has been deprecated

            var cts = new CancellationTokenSource();
            Random _rnd = new Random(DateTime.Now.Millisecond);

            await Task.Run(() =>
            {
                if (Console.ReadKey().KeyChar == 'c')
                    cts.Cancel();
            }, cts.Token);


            // Will stop accepting new items when the number of items exceeds 5
            var inputBlock = new BufferBlock<int>(
              new DataflowBlockOptions { BoundedCapacity = 5, CancellationToken = cts.Token });

            var convertToDecimalBlock = new TransformBlock<int, decimal>(
              n =>
              {
                  decimal result = Convert.ToDecimal(n * 100);
                  Console.WriteLine($"Decimal Converter sent {result} to the next stage on " +
                $"thread id {Thread.CurrentThread.ManagedThreadId}");
                  Thread.Sleep(TimeSpan.FromMilliseconds(_rnd.Next(200)));
                  return result;
              }
              , new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token });

            var stringifyBlock = new TransformBlock<decimal, string>(
              n =>
              {
                  string result = $"--{n.ToString("C", CultureInfo.GetCultureInfo("en-us"))}--";
                  Console.WriteLine($"String Formatter sent {result} to the next stage on thread id {Thread.CurrentThread.ManagedThreadId}");
                  Thread.Sleep(TimeSpan.FromMilliseconds(_rnd.Next(200)));
                  return result;
              }
              , new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token });

            var outputBlock = new ActionBlock<string>(
              s =>
              {
                  Console.WriteLine($"The final result is {s} on thread id {Thread.CurrentThread.ManagedThreadId}");
              }
              , new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token });

            inputBlock.LinkTo(convertToDecimalBlock, new DataflowLinkOptions { PropagateCompletion = true });
            convertToDecimalBlock.LinkTo(stringifyBlock, new DataflowLinkOptions { PropagateCompletion = true });
            stringifyBlock.LinkTo(outputBlock, new DataflowLinkOptions { PropagateCompletion = true });

            try
            {
                Parallel.For(0, 20, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token }
                , i =>
                {
                    Console.WriteLine($"added {i} to source data on thread id {Thread.CurrentThread.ManagedThreadId}");
                    inputBlock.SendAsync(i).GetAwaiter().GetResult();
                });
                inputBlock.Complete();
                await outputBlock.Completion;
                Console.WriteLine("Press ENTER to exit.");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation has been canceled! Press ENTER to exit.");
            }

            Console.ReadLine();
        }

        private static void Worker(IHasValue state)
        {
            Console.WriteLine($"Worker runs on thread id {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine(value: $"State value: {state.Value.Text}");
        }

        static void CreateInitialValues( BlockingCollection<int>[] sourceArrays,  CancellationTokenSource cts)
        {
            Parallel.For(0, sourceArrays.Length * Count, (j, state) => {
                if (cts.Token.IsCancellationRequested)
                {
                    state.Stop();
                }

                int number = GetRandomNumber(j);

                int k = BlockingCollection<int>.TryAddToAny(sourceArrays, j);

                if( k >= 0)
                {
                    Console.WriteLine($"added {j} to source data on thread id {Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(TimeSpan.FromMilliseconds(number));
                }
            });

            foreach (var arr in sourceArrays)
            {
                arr.CompleteAdding();
            }
        }

        private static int GetRandomNumber(int seed)
        {
            return new Random(seed).Next(500);
        }

        private static async Task<string> ProcessBook(string bookContent, string title, HashSet<string> stopwords)
        {
            using (var reader = new StringReader(bookContent))
            {
                var query =  reader.EnumLines()
                    .AsParallel()
                    .SelectMany(line => line.Split(delimiters))
                    .MapReduce(
                        word => new[] { word.ToLower() },
                        key => key,
                        g => new[] { new { Word = g.Key, Count = g.Count() } }
                    )
                    .ToList();

                var words = query
                    .Where(element =>
                        !string.IsNullOrWhiteSpace(element.Word)  && !stopwords.Contains(element.Word))
                    .OrderByDescending(element => element.Count);

                var sb = new StringBuilder();

                sb.AppendLine($"'{title}' book stats");
                sb.AppendLine("Top ten words used in this book: ");
                foreach (var w in words.Take(10))
                {
                    sb.AppendLine($"Word: '{w.Word}', times used: '{w.Count}'");
                }

                sb.AppendLine($"Unique Words used: {query.Count()}");

                return sb.ToString();
            }
        }

        private async Task<string> DownloadBookAsync(string bookUrl)
        {
            using(var client = new HttpClient())
            {
                return await client.GetStringAsync(bookUrl);
            }
        }

        private async Task<HashSet<string>> DownloadStopWordsAsync()
        {
            string url = "https://raw.githubusercontent.com/6/stopwords/master/stopwords-all.json";

            using (var client = new HttpClient())
            {
                try
                {
                    var content = await client.GetStringAsync(url);
                    var words = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);

                    return new HashSet<string>(words["en"]);
                }
                catch 
                {

                    return new HashSet<string>();
                }
            }
            
        }

        // ************************
        public void ImplementingLazyEvaluatedsharedsstates()
        {
            var t = ProcessAsynchronously();
            t.GetAwaiter().GetResult();
        }

        public void ImplementingParallelPipelinewithBlockingCollection()
        {
            var cts = new CancellationTokenSource();

            Task.Run( () => {
                if (Console.ReadKey().KeyChar ==  'c')
                {
                    cts.Cancel();
                }
            }, cts.Token);

            var sourceArrays = new BlockingCollection<int>[CollectionsNumber];

            for (int i = 0; i < sourceArrays.Length; i++)
            {
                sourceArrays[i] = new BlockingCollection<int>(Count);
            }

            var convertToDecimal = new PipelineWorker<int, decimal>(sourceArrays, n => Convert.ToDecimal(n*100), cts.Token, "Decimal Converter");

            var stringifyNumber = new PipelineWorker<decimal, string>(convertToDecimal.Output,
                                                                                                                   s => $"--{s.ToString("C", CultureInfo.GetCultureInfo("en-us"))}--",
                                                                                                                    cts.Token, 
                                                                                                                    "String Formatter");

            var outputResultToConsole = new PipelineWorker<string, string> ( stringifyNumber.Output,
                                                                                                                             s => Console.WriteLine($"The final result is {s} on thread id {Thread.CurrentThread.ManagedThreadId}"),
                                                                                                                             cts.Token,
                                                                                                                            "Console Output"  );

            try
            {
                Parallel.Invoke(  () =>   CreateInitialValues(sourceArrays, cts),
                                            () => convertToDecimal.Run(),
                                            () => stringifyNumber.Run(),
                                            () => outputResultToConsole.Run() );
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                    Console.WriteLine(ex.Message + ex.StackTrace);
            }

            if (cts.Token.IsCancellationRequested)
            {
                Console.WriteLine("Operation has been canceled! Press ENTER to exit.");
            }
            else
            {
                Console.WriteLine("Press ENTER to exit.");
            }
            Console.ReadLine();
        }

        public void ImplementingParallelPipelineWithTPLDataFlow()
        {
            var t = ProcessAsynchronously();
            t.GetAwaiter().GetResult();
        }

        public void ImplementingMapReduceWithPLINQ()
        {
            var booksList = new Dictionary<string, string>()
            {
                ["Moby Dick; Or, The Whale by Herman Melville"] = "http://www.gutenberg.org/cache/epub/2701/pg2701.txt",

                ["The Adventures of Tom Sawyer by Mark Twain"]  = "http://www.gutenberg.org/cache/epub/74/pg74.txt",

                ["Treasure Island by Robert Louis Stevenson"]  = "http://www.gutenberg.org/cache/epub/120/pg120.txt",

                ["The Picture of Dorian Gray by Oscar Wilde"] = "http://www.gutenberg.org/cache/epub/174/pg174.txt"
            };

            HashSet<string> stopwords = DownloadStopWordsAsync().GetAwaiter().GetResult();

            var output = new StringBuilder();

            Parallel.ForEach(booksList.Keys, key =>
            {
                var bookContent = DownloadBookAsync(booksList[key])
                    .GetAwaiter().GetResult();

                string result = ProcessBook(bookContent, key, stopwords)
                    .GetAwaiter().GetResult();

                output.Append(result);
                output.AppendLine();
            });

            Console.Write(output.ToString());
            Console.ReadLine();
        }
    }
}
