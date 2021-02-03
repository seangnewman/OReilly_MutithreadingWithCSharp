using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter4_UsingTaskParallelLibrary
{
    class Examples
    {

        delegate string AsynchronousTask(string threadName);
        delegate string IncompatibleAsynchronousTask(out int threadId);

        //static void TaskMethod(string name)
        //{
        //    Console.WriteLine($"Task {name} is running n a thread id {Thread.CurrentThread.ManagedThreadId}.  Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
        //}

        static int TaskMethod(string name)
        {
            Console.WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}.  Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            return 42;
        }

        static Task<int> CreateTask(string name)
        {
            return new Task<int>(  () => TaskMethod(name));


        }
       
        static int TaskMethod(string name, int seconds)
        {
            Console.WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}.  Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            throw new Exception("Boom!");
            return 42 * seconds;
        }

        static int TaskMethod(string name, int seconds, CancellationToken token)
        {
            Console.WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");

            for (int i = 0; i < seconds; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                if (token.IsCancellationRequested)
                {
                    return -1;
                }
            }

            return 42 * seconds;
        }

        static void Callback(IAsyncResult ar)
        {
            Console.WriteLine("Starting a callback....");
            Console.WriteLine($"State passed to a callback: {ar.AsyncState}");
            Console.WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Console.WriteLine($"Thread pool worker thread id: {Thread.CurrentThread.ManagedThreadId}");
        }

        static string Test(string threadName)
        {
            Console.WriteLine("Starting ....");
            Console.WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Thread.CurrentThread.Name = threadName;
            return $"Thread name : {Thread.CurrentThread.Name}";
        }

        static string Test(out int threadId)
        {
            Console.WriteLine("Starting... ");
            Console.WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            threadId = Thread.CurrentThread.ManagedThreadId;
            return $"Thread pool worker thread id was {threadId}";
        }

        //  ----------------------------------------------------------
        public  void CreatingATask()
        {
           
            var t1 = new Task(() => TaskMethod("Task 1"));
            var t2 = new Task(() => TaskMethod("Task 2"));

            t2.Start();
            t1.Start();

            Task.Run(() => TaskMethod("Task 3"));

            Task.Factory.StartNew(() => TaskMethod("Task 4"));
            Task.Factory.StartNew(() => TaskMethod("Task 5"), TaskCreationOptions.LongRunning);  // Task run on a separate thread that does not use a thread pool

            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        public void BasicOperations()
        {
            TaskMethod("Main Thread Task");

              Task<int> task = CreateTask("Task 1");
            task.Start();
            int result = task.Result;
            Console.WriteLine($"Result is {result}");

            task = CreateTask("Task 2");
            task.RunSynchronously();
            result = task.Result;
            Console.WriteLine($"Result is {result}");

            task = CreateTask("Task 3");
            Console.WriteLine(task.Status);
            task.Start();

            while (task.IsCompleted)
            {
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }

            Console.WriteLine(task.Status);
            result = task.Result;
            Console.WriteLine($"Result is: {result}");

        }

       public  void CombiningTasks()
        {
            var firstTask = new Task<int>( () => TaskMethod("First Task", 3));
            var secondTask = new Task<int>( () => TaskMethod("Second Task", 2));

            firstTask.ContinueWith(t => 
               Console.WriteLine($"The first answer is {t.Result}. Thread id {Thread.CurrentThread.ManagedThreadId}, is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}")
               , TaskContinuationOptions.OnlyOnRanToCompletion);

            firstTask.Start();
            secondTask.Start();


            Task continuation = secondTask.ContinueWith(t => Console.WriteLine($"The second answer is {t.Result}. Thread id {Thread.CurrentThread.ManagedThreadId}, is thread pool thread: " +
                $" {Thread.CurrentThread.IsThreadPoolThread}")
                ,TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);

            continuation.GetAwaiter().OnCompleted( () =>
                Console.WriteLine($"Continuation Task Completed! Thread id {Thread.CurrentThread.ManagedThreadId}, is thread pool thread: " +
                $"{Thread.CurrentThread.IsThreadPoolThread}"));

            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine();


            // ------- Parent/Child Task Relationships
            firstTask = new Task<int>( () => {
                var innerTask = Task.Factory.StartNew( () => TaskMethod("Second Task", 5), TaskCreationOptions.AttachedToParent);

                innerTask.ContinueWith( t => TaskMethod("Third Task", 2), TaskContinuationOptions.AttachedToParent);    // Parent will not complete until child has completed

                return TaskMethod("First Task", 2); 
            } );

            firstTask.Start();

            while (!firstTask.IsCompleted)
            {
                Console.WriteLine(firstTask.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }

            Console.WriteLine(firstTask.Status);
            Thread.Sleep(TimeSpan.FromSeconds(10));

        }

        public void ConvertingAPMToTasks()
        {
            int threadId;
            AsynchronousTask d = Test;
            IncompatibleAsynchronousTask e = Test;

            Console.WriteLine("Option 1");
            Task<string> task = Task<string>.Factory.FromAsync( d.BeginInvoke("AsyncTaskThead", Callback,   "a delegate asynchronous call"),
                                                                                            d.EndInvoke );
            task.ContinueWith(t => Console.WriteLine($"Callback is finished, now running a continuation!  Result: {t.Result}"));

            while (!task.IsCompleted)
            {
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }

            Console.WriteLine(task.Status);
            Thread.Sleep(TimeSpan.FromSeconds(2));

            Console.WriteLine(" ---------------------------------------------------- ");
            Console.WriteLine();
            Console.WriteLine("Option 2");

            task = Task<string>.Factory.FromAsync(d.BeginInvoke, d.EndInvoke, "AsyncTaskThread", "a delegate asynchronous call");
            task.ContinueWith(t => Console.WriteLine($"Task is completed, no running a continuation! Result: {t.Result}"));
            while (!task.IsCompleted)
            {
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine(task.Status);
            Thread.Sleep(TimeSpan.FromSeconds(1));

            Console.WriteLine(" ---------------------------------------------------- ");
            Console.WriteLine();
            Console.WriteLine("Option 3");

            IAsyncResult ar = e.BeginInvoke(out threadId, Callback, "a delegate asynchronous call");
            task = Task<string>.Factory.FromAsync( ar, _=> e.EndInvoke(out threadId, ar));

            task.ContinueWith(t => Console.WriteLine($"Task is completed, now running a continuation! Result : {t.Result}, ThreadId : {threadId}"));

            while (!task.IsCompleted)
            {
                Console.WriteLine( task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));

            }

            Console.WriteLine(task.Status);
            Thread.Sleep(TimeSpan.FromSeconds(1));

        }

        public void ConvertingEAPToTasks()
        {
            var tcs = new TaskCompletionSource<int>();

            var worker = new BackgroundWorker();

            worker.DoWork += (sender, EventArgs) => EventArgs.Result = TaskMethod("Background workier", 5);

            worker.RunWorkerCompleted += (sender, EventArgs) => {
                if (EventArgs.Error != null)
                {
                    tcs.SetException(EventArgs.Error);
                }
                else if (EventArgs.Cancelled)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    tcs.SetResult((int)EventArgs.Result);
                }
            
            };

            worker.RunWorkerAsync();
            int result = tcs.Task.Result;

            Console.WriteLine($"Result is: {result}");

        }

        public void CancellationOption()
        {
            var cts = new CancellationTokenSource();

            // cts token is passed tice.   If the task is cancelled before it is started,  the code is not executed at all and TPL handles cancellation
            // If it is started then cancelled, then the code in TaskMethod has to handle
            var longTask = new Task<int>( () => TaskMethod("Task 1", 10, cts.Token), cts.Token);
            Console.WriteLine(longTask.Status);
            cts.Cancel();
            Console.WriteLine(longTask.Status);
            Console.WriteLine("First task has been cancelled prior to execution");

            cts = new CancellationTokenSource();
            longTask = new Task<int>( () => TaskMethod("Task 2", 10, cts.Token), cts.Token);
            longTask.Start();

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine(longTask.Status);
            }

            cts.Cancel();

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine(longTask.Status);
            }

            Console.WriteLine($"A task has been completed with result {longTask.Result}.");
        }

        public void HandlingExceptions()
        {
            Task<int> task;

            try
            {
                task = Task.Run( () => TaskMethod("Task 1", 2) );
                int result = task.Result;
                Console.WriteLine($"Result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceptin caught : {ex}");
                 
            }

            Console.WriteLine("-----------------------------------------");
            Console.WriteLine();

            try
            {
                task = Task.Run(() => TaskMethod("Task 2", 2));
                int result = task.GetAwaiter().GetResult();
                Console.WriteLine($"Result : {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught : {ex}");
            }

            Console.WriteLine("-----------------------------------------");
            Console.WriteLine();

            var t1 = new Task<int>( () => TaskMethod("Task 3", 3));
            var t2 = new Task<int>(() => TaskMethod("Task 4", 2));

            var complexTask = Task.WhenAll(t1, t2);


            // Using Flatten() returns a collection of all inner exceptions, not just the outside exception
            var exceptionHandler = complexTask.ContinueWith(t => Console.WriteLine($"Exception caught: {t.Exception.Flatten()}"), TaskContinuationOptions.OnlyOnFaulted);

            t1.Start();
            t2.Start();

            Thread.Sleep(TimeSpan.FromSeconds(5));


        }

    }
}
