using System;
using System.Diagnostics;
using System.Dynamic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Linq;
using ImpromptuInterface;

namespace Chapter6_UsingCSharp6
{
    class Examples
    {
        private static  Label _label;

        static async void Click(object sender, EventArgs e)
        {
            _label.Content = new TextBlock { Text = "Calculating ..."};

            TimeSpan resultWithContext = await Test();
            TimeSpan resultNoContext = await TestNoContext();

            // TimeSpan resultNoContext = await TestNoContext(), ConfigureAwait(false);

            var sb = new StringBuilder();
            sb.AppendLine($"With the context: {resultWithContext}");
            sb.AppendLine($"Without the context: {resultNoContext}");
            sb.AppendLine($"Ratio : {resultWithContext.TotalMilliseconds/resultNoContext.TotalMilliseconds: 0.00}");
            _label.Content = new TextBlock { Text = sb.ToString()};
        }

        private static async Task<TimeSpan> Test()
        {
            const int iterationsNumber = 1000000;
            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < iterationsNumber; i++)
            {
                var t = Task.Run( () => { });
                await t;
            }

            sw.Stop();
            return sw.Elapsed;
        }

        private async static Task<TimeSpan> TestNoContext()
        {
            const int iterationsNumber = 1000000;
            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < iterationsNumber; i++)
            {
                var t = Task.Run(() => { });
                await t.ConfigureAwait( continueOnCapturedContext: false);
            }

            sw.Stop();
            return sw.Elapsed;
        }

        //static Task AsynchronouWithTPL()
        //{
        //    Task<string> t = GetInfoAsync("Task 1");

        //    Task t2 = t.ContinueWith(task => Console.WriteLine(t.Result), TaskContinuationOptions.NotOnFaulted);
        //    Task t3 = t.ContinueWith(task => Console.WriteLine(t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted);

        //    return Task.WhenAny(t2, t3);
        //}

        static Task AsynchronouWithTPL()
        {
            var containerTask = new Task(() =>
            {
                Task<string> t = GetInfoAsync("TPL 1");

                t.ContinueWith(task =>
                {
                    Console.WriteLine(t.Result);
                    Task<string> t2 = GetInfoAsync("TPL 2");
                    t2.ContinueWith(innerTask => Console.WriteLine(innerTask.Result), TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.AttachedToParent);
                    t2.ContinueWith(innerTask => Console.WriteLine(innerTask.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent);
                }, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.AttachedToParent);

                t.ContinueWith(task => Console.WriteLine(t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent);

            });

            containerTask.Start();
            return containerTask;
        }


        //private async static Task<string> GetInfoAsync(string name)
        //{
        //    await Task.Delay(TimeSpan.FromSeconds(2));

        //    //throw new Exception("Boom!");
        //    return $"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
        //}

        private async static Task<string> GetInfoAsync(string name)
        {
            Console.WriteLine($"Task {name} started!");
            await Task.Delay(TimeSpan.FromSeconds(2));
            //if (name == "TPL 2")
            //{
            //    throw new Exception("Boom!");
            //}
            return $"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
        }

        //private static async Task<string> GetInfoAsync(string name, int seconds)
        //{
        //    await Task.Delay(TimeSpan.FromSeconds(2));

        //    //await Task.Run( () => {
        //    //    Thread.Sleep(TimeSpan.FromSeconds(2));
        //    //});

        //    return $"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
        //}

        //private static async Task<string> GetInfoAsync(string name, int seconds)
        //{
        //    await Task.Delay(TimeSpan.FromSeconds(2));
        //    throw new Exception($"Boom from {name}");
        //}

        private static async Task<string> GetInfoAsync(string name, int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));

            if (name.Contains("Exception"))
            {
                throw new Exception("Boom!  From {name}");
            }

            return $"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}.  Is thread poop thread? {Thread.CurrentThread.IsThreadPoolThread}";

            
        }

        //private async Task AsynchronyWithAwait()
        //{
        //    try
        //    {
        //        string result = await GetInfoAsync("Task 2");
        //        Console.WriteLine(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //}

        private async Task AsynchronyWithAwait()
        {
            try
            {
                string result = await GetInfoAsync("Async 1");
                Console.WriteLine(result);

                result = await GetInfoAsync("Async 2");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        //private static async Task AsynchronousProcessing()
        //{
        //    Func<string, Task<string>> asyncLambda = async (name) =>
        //    {
        //        await Task.Delay(TimeSpan.FromSeconds(2));
        //        return
        //          $"Task {name} is running on thread id {Thread.CurrentThread.ManagedThreadId} Is thread pool thread {Thread.CurrentThread.IsThreadPoolThread}";
        //    };

        //    string result = await asyncLambda("async lambda");

        //    Console.WriteLine(result);
        //}

        //private static async Task AsynchronousProcessing()
        //{
        //    Task<string> t1 = GetInfoAsync("Task 1", 3);
        //    Task<string> t2 = GetInfoAsync("Task 2", 5);

        //    string[] results = await Task.WhenAll(t1, t2);
        //    foreach (var result in results)
        //    {
        //        Console.WriteLine(result);
        //    }
        //}

        //private static async Task AsynchronousProcessing()
        //{
        //    Console.WriteLine("1. Sngle exception");

        //    try
        //    {
        //        string result = await GetInfoAsync("Task 1", 2);
        //        Console.WriteLine(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception details: {ex}");
        //        throw;
        //    }

        //    Console.WriteLine();
        //    Console.WriteLine("2. Multiple exceptions");

        //    Task<string> t1 = GetInfoAsync("Task 1", 3);
        //    Task<string> t2 = GetInfoAsync("Task 2", 2);

        //    Task<string[]> t3 = Task.WhenAll(t1, t2);

        //    try
        //    {
        //        string[] results = await t3;
        //        Console.WriteLine(results.Length);
        //    }
        //    catch 
        //    {
        //        var ae = t3.Exception.Flatten();
        //        var exceptions = ae.InnerExceptions;
        //        Console.WriteLine($"Exceptions caught: {exceptions.Count}");

        //        foreach (var e in exceptions)
        //        {
        //            Console.WriteLine($"Exception details: {e}");
        //            Console.WriteLine();
        //        }

        //    }

        //    Console.WriteLine();
        //    Console.WriteLine("4. await in catch and finally blocks");

        //    try
        //    {
        //        string result = await GetInfoAsync("Task 1", 2);
        //        Console.WriteLine(result);
        //    }
        //    catch (Exception ex)
        //    {

        //        await Task.Delay(TimeSpan.FromSeconds(1));
        //        Console.WriteLine($"Catch block with await: Exception details: {ex}");
        //    }
        //    finally
        //    {
        //        await Task.Delay(TimeSpan.FromSeconds(2));
        //        Console.WriteLine("Finally block");
        //    }
        //}

        //private static async Task AsynchronousProcessing()
        //{
        //    var sync = new CustomAwaitable(true);
        //    string result = await sync;
        //    Console.WriteLine(result);

        //    var async = new CustomAwaitable(false);
        //    result = await async;
        //    Console.WriteLine(result);


        //}
        private static async Task AsynchronousProcessing()
        {
            string result = await GetDynamicAwaitableObject(true);
            Console.WriteLine(result);

            result = await GetDynamicAwaitableObject(false);
            Console.WriteLine(result);
        }

        private static dynamic  GetDynamicAwaitableObject(bool completeSynchronously)
        {
            dynamic result = new ExpandoObject();    // Creates a dictionary with key string and a  (func<string> delegate) 
            dynamic awaiter = new ExpandoObject();  

            awaiter.Message = "Completed synchronously";
            awaiter.IsCompleted = completeSynchronously;

            awaiter.GetResult = (Func<string>)(() => awaiter.Message); 

            awaiter.OnCompleted = (Action<Action>)(callback => ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                awaiter.Message = GetInfo();
                callback?.Invoke();

            }));

            // Implements INotifyCompletion
            //Impormptu allos the method to dynamically create a proxy
            IAwaiter<string> proxy = Impromptu.ActLike(awaiter);

            result.GetAwaiter = (Func<dynamic>)( () => proxy);

            return result;
        }

        private static dynamic GetInfo()
        {
            // awaitable object requires  GetInfo method
            return $"Task is running on thread id {Thread.CurrentThread.ManagedThreadId} Is thread pool thread {Thread.CurrentThread.IsThreadPoolThread}";
        }

        static async Task AsyncTaskWithErrors()
        {
            string result = await GetInfoAsync("AsyncTaskExceptions", 2);
            Console.WriteLine(result);
        }

        static async void AsyncVoidWithErrors()
        {
            string result = await GetInfoAsync("AsyncVoidExceptions", 2);
            Console.WriteLine(result);
        }

        static async Task AsyncTask()
        {
            string result = await GetInfoAsync("AsyncTask", 2);
            Console.WriteLine(result);
        }

        static async void AsyncVoid()
        {
            string result = await GetInfoAsync("AsyncVoid", 2);
            Console.WriteLine(result);

        }

        // ---------------------------------------------
        public void UsingAwaitToGetAsyncTaskResults()
        {
            Task t = AsynchronouWithTPL();
            t.Wait();                                                   // Not a best practice to use Wait or Result

            t = AsynchronyWithAwait();
            t.Wait();                                                   // Not a best practice to use Wait or Result
        }

        public void UsingAwaitOperatorInLambdaExpression()
        {
            Task t = AsynchronousProcessing();
            t.Wait();
        }

        public void UsingAwaitWithConsequentAsyncTasks()
        {
            Task t = AsynchronouWithTPL();
            t.Wait();                                                   // Not a best practice to use Wait or Result

            t = AsynchronyWithAwait();
            t.Wait();                                                   // Not a best practice to use Wait or Result
        }

        public void UsingAwaitForParallelTasks()
        {
            Task t = AsynchronousProcessing();
            t.Wait();                                                   // Not a best practice to use Wait or Result
        }

        public void HandlingExceptionsInAsyncOperations()
        {
            Task t = AsynchronousProcessing();
            t.Wait();
        }

        public void AvoidingUseOfCapturedSyncThread()
        {
            var app = new Application();
            var win = new Window();
            var panel = new StackPanel();
            var button = new Button();

            _label = new Label
            {
                FontSize = 32,
                Height = 200
            };

            button.Height = 100;
            button.FontSize = 32;
            button.Content = new TextBlock { Text = "Start asynchronous operations" };
            button.Click += Click;

            panel.Children.Add(_label);
            panel.Children.Add(button);

            win.Content = panel;

            app.Run(win);

            Console.ReadLine();

        }

        public void WorkingAroundAsyncVoid()
        {
            Task t = AsyncTask();
            t.Wait();

            AsyncVoid();
            Thread.Sleep(TimeSpan.FromSeconds(3));

            t = AsyncTaskWithErrors();

            while (!t.IsFaulted)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Console.WriteLine(t.Exception);

            //try
            //{
            //    AsyncVoidWithErrors();
            //    Thread.Sleep(TimeSpan.FromSeconds(3));
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}

            int[] numbers = { 1, 2, 3, 4, 5 };

            Array.ForEach( numbers, async number => {
                await Task.Delay(TimeSpan.FromSeconds(1));

                if (number == 3)
                {
                    throw new Exception("Boom!");
                    Console.WriteLine(number);
                }
            } );

            Console.ReadLine();


        }

        public void DesigningCustomAwaitableTypes()
        {
            Task t = AsynchronousProcessing();
            t.Wait();
        }
    }
}
