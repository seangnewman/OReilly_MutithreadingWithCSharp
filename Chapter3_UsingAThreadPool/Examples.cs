using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter3_UsingAThreadPool
{
    class Examples
    {
        static  Timer _timer;

        private static void Callback(IAsyncResult ar)
        {
            Console.WriteLine("Starting a callback...");
            Console.WriteLine($"State passed to callback: {ar.AsyncState}");
            Console.WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Console.WriteLine($"Thread pool worker thread id: {Thread.CurrentThread.ManagedThreadId}");
        }

        private static string Test (out int threadID)
        {
            Console.WriteLine("Starting ....");
            Console.WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            threadID = Thread.CurrentThread.ManagedThreadId;
            return $"Thread pool worker thread id was {threadID}";
        }

        private static void AsyncOperation(object state)
        {
            Console.WriteLine($"Operation state: {state ?? "{null}"}");
            Console.WriteLine($"Worker thread id: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        private static void UseThreads(int numberOfOperations)
        {
            using(var countdown = new CountdownEvent(numberOfOperations))
            {
                Console.WriteLine("Scheduling work by creating threads");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    var thread = new Thread( () => {
                        Console.Write($"{Thread.CurrentThread.ManagedThreadId},");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countdown.Signal();
                    });
                    thread.Start();
                }
                countdown.Wait();
                Console.WriteLine();
            }
        }

        static void UseThreadPool(int numberOfOperations)
        {
            using(var countdown = new CountdownEvent(numberOfOperations))
            {
                Console.WriteLine("Starting work on a threadpool");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    ThreadPool.QueueUserWorkItem(_=> {
                        Console.Write($"{Thread.CurrentThread.ManagedThreadId},");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countdown.Signal();
                    });
                }
                countdown.Wait();
                Console.WriteLine();

            }
        }

        static void AsyncOperation1(CancellationToken token)
        {

            //Poll and  check the isCancellationRequested property 
            Console.WriteLine("Starting the first task");
            for (int i = 0; i < 5; i++)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("The first task has been cancelled");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Console.WriteLine("The first task has completed successfully");
        }

        static void AsyncOperation2(CancellationToken token)
        {
            //Throw the OperationCancelledException
            try
            {
                Console.WriteLine("Starting the second task");
                for (int i = 0; i < 5; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                Console.WriteLine("The second task has completed successfully");
            }
           
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("The second task has been canceled : " + ex.Message);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        static void AsyncOperation3(CancellationToken token)
        { 
            //Register the callback  to chain the cancellation logic
            bool cancellationFlag = false;
            token.Register(() => {
                cancellationFlag = true;
            });
            Console.WriteLine("Starting the third task");

            for (int i = 0; i < 5; i++)
            {
                if (cancellationFlag)
                {
                    Console.WriteLine("The third task has been cancelled");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Console.WriteLine("The third task has been completed successfully");
        }

        static  void RunOperations(TimeSpan workerOperationTimeout)
        {
          
            using (var evt = new ManualResetEvent(false))
            {
                using (var cts = new CancellationTokenSource())
                {
                    Console.WriteLine("Registering timeout operation.....");

                    //Callback occurs when a signal or timeout accurs
                    var worker = ThreadPool.RegisterWaitForSingleObject(
                                                                    evt
                                                                    , (state, isTimedOut) => WorkerOperationWait(cts, isTimedOut)
                                                                    , null
                                                                    , workerOperationTimeout
                                                                    , true);
                    Console.WriteLine("Starting long running operation.....");
                    ThreadPool.QueueUserWorkItem(_ => WorkerOperation(cts.Token, evt));

                    Thread.Sleep(workerOperationTimeout.Add(TimeSpan.FromSeconds(2)));
                    worker.Unregister(evt);
                }
            }
        }

        static  void WorkerOperation(CancellationToken token, ManualResetEvent evt)
        {
            for (int i = 0; i < 6; i++)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            evt.Set();
        }

        static void WorkerOperationWait(CancellationTokenSource cts, bool isTimedOut)
        {
            if (isTimedOut)
            {
                cts.Cancel();
                Console.WriteLine("Worker operation timed out and was cancelled");
            }
            else
            {
                Console.WriteLine("Worker operation succeeded.");
            }
        }

        static void TimerOperation(DateTime start)
        {
            TimeSpan elapsed = DateTime.Now - start;
            Console.WriteLine($"{elapsed.Seconds} seconds from {start}.  Timer thread pool thread id {Thread.CurrentThread.ManagedThreadId}");
        }

        static void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine($"DoWork thread pool thread id: {Thread.CurrentThread.ManagedThreadId}");
            var bw = (BackgroundWorker)sender;

            for (int i = 1; i <= 100; i++)
            {
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (i % 10 == 0)
                {
                    bw.ReportProgress(i);
                }

                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }
            e.Result = 42;
        }

        static void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine($"{e.ProgressPercentage}% completed.  Progress thread pool thread id: {Thread.CurrentThread.ManagedThreadId}");
        }

        static void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine($"Completed pool thread pool thread id: {Thread.CurrentThread.ManagedThreadId}");

            if (e.Error != null)
            {
                Console.WriteLine($"Exception {e.Error.Message} has occurred.");
            }
            else if (e.Cancelled)
            {
                Console.WriteLine($"Operation has been cancelled");
            }
            else
            {
                Console.WriteLine($"The answer is {e.Result}");
            }
        }

        //-------------------------------------------------------------------------

        public void InvokingADelegateOnThreadPool()
        {
            int threadId = 0;

            RunOnThreadPool poolDelegate = Test;

            var t = new Thread(() =>
            {
                Test(out threadId);
            });
            t.Start();
            t.Join();

            Console.WriteLine($"Thread id : {threadId}");

            //APM  (Asynchronous Programming Model)
            IAsyncResult r = poolDelegate.BeginInvoke(out threadId, Callback, "a delegate asynchronous call");
            r.AsyncWaitHandle.WaitOne();    // waits on operation until completed

            string result = poolDelegate.EndInvoke(out threadId, r);
            // End APM Pattern
            // Note the preference is the TPL Model

            Console.WriteLine($"Thread pool worker thread id: {threadId}");
            Console.WriteLine(result);

            Thread.Sleep(TimeSpan.FromSeconds(2));


        }

        public void PostingOnAThreadPool()
        {
            const int x = 1;
            const int y = 2;
            const string lambdaState = "lambda state 2";
            // Post method on thread pool
            ThreadPool.QueueUserWorkItem(AsyncOperation, "async state");
            //Sleep for one second to allow reuse of pools
            Thread.Sleep(TimeSpan.FromSeconds(1));


            //Post again with state object
            ThreadPool.QueueUserWorkItem(state => {
                Console.WriteLine($"Operation state: {state}");
                Console.WriteLine($"Worker thread id: {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(TimeSpan.FromSeconds(2));

            }, "lambda state");
           
            ThreadPool.QueueUserWorkItem( _ => {
                Console.WriteLine($"Operation state: {x + y}, {lambdaState}");
                Console.WriteLine($"Worker thread id {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }, "lambda state");

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        public void ThreadPoolAndDegreeOfParallelism()
        {
            const int numberOfOperations = 500;
            var sw = new Stopwatch();
            sw.Start();

            UseThreads(numberOfOperations);
            sw.Stop();
            Console.WriteLine($"Execution time using threads:  {sw.ElapsedMilliseconds}");

            sw.Reset();
            sw.Start();
            //Using the thread pool saves memory and reduces the number of threads
            // But pays with it in performance
            UseThreadPool(numberOfOperations);
            sw.Stop();
            Console.WriteLine($"Execution time using the thread pool: {sw.ElapsedMilliseconds}");

        }

        public void ImplementingCancellationOption()
        {
            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem( _ => {
                    AsyncOperation1(token);
                });
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => {
                    AsyncOperation2(token);
                });
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => {
                    AsyncOperation3(token);
                });
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        public void WaitHandleAndTimeout()
        {
            RunOperations(TimeSpan.FromSeconds(5));
            RunOperations(TimeSpan.FromSeconds(7));
        }

        public void UsingATimer()
        {
            Console.WriteLine("'Press Enter' to stop the timer...");
            DateTime start = DateTime.Now;
            _timer = new Timer( _ => TimerOperation(start)
                                                                                         , null
                                                                                         ,TimeSpan.FromSeconds(1)    // Start in 1 second
                                                                                         ,TimeSpan.FromSeconds(2) );  // Run for 2 seconds
            try
            {
                Thread.Sleep(TimeSpan.FromSeconds(6));
                _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));
                Console.ReadLine();  // Press Enter to end 
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                _timer.Dispose();
            }

    }

        public void BackgroundWorkerComponent()
        {
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            // EAP Event-based Asynchronous Pattern
            // Subscriber to the three events and corresponding event handlers
            bw.DoWork += Worker_DoWork;
            bw.ProgressChanged += Worker_ProgressChanged;
            bw.RunWorkerCompleted += Worker_Completed;
            
             // End EAP Pattern

            bw.RunWorkerAsync();

            Console.WriteLine("Press C to cancel work");

            do
            {
                if (Console.ReadKey(true).KeyChar == 'C')
                {
                    bw.CancelAsync();
                }
            } while (bw.IsBusy);

        }

      
    }

    delegate string RunOnThreadPool(out int threadID);
}
