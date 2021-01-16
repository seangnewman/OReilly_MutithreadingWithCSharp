using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter2_Synchronization
{
    class Examples
    {
        // restricts access to four threads
        static SemaphoreSlim _semaphore = new SemaphoreSlim(4);

        private static AutoResetEvent _workerEvent = new AutoResetEvent(false);
        private static AutoResetEvent _mainEvent = new AutoResetEvent(false);

        private static ManualResetEventSlim _mainEventSlim = new ManualResetEventSlim(false);
        
        static void TestCounter(CounterBase c)
        {
            for (int i = 0; i < 1000000; i++)
            {
                c.Increment();
                c.Decrement();
            }
        }

        static void AccessDataBase(string name, int seconds) 
        {
            Console.WriteLine($"{ name} waits to access a database");
            _semaphore.Wait();
            Console.WriteLine($"{name} was granted an access to a database");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine($"{name} is completed");
            _semaphore.Release();
        }

        static void Process(int seconds)
        {
            Console.WriteLine("Starting a long running work....");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("Work is done!");
            _workerEvent.Set();

            Console.WriteLine("Waiting for a main thread to complete its work");
            _mainEvent.WaitOne();

            Console.WriteLine("Starting a second operation....");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("Work is done!");
            _workerEvent.Set();

        }

        static void TravelThroughGates(string threadName, int seconds)
        {
            Console.WriteLine($"{threadName} falls to sleep" );
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine($"{threadName} waits for the gates to open!");
            _mainEventSlim.Wait();
            Console.WriteLine($"{threadName} enters the gates!");
        }
        // ----------------------------------------------
        public void BasicAtomicOperations()
        {
            Console.WriteLine("Increment counter");
            var c = new Counter();

            var t1 = new Thread( () => {
                TestCounter(c);
            });
            var t2 = new Thread(() => {
                TestCounter(c);
            });
            var t3 = new Thread(() => {
                TestCounter(c);
            });

            t1.Start();
            t2.Start();
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();

            Console.WriteLine($"Total count:  {c.Count}");
            Console.WriteLine("---------------------------------");

            Console.WriteLine("Correct counter");
            var c1 = new CounterLNoLock();

            t1 = new Thread(() => {
                TestCounter(c1);
            });
            t2 = new Thread(() => {
                TestCounter(c1);
            });
            t3 = new Thread(() => {
                TestCounter(c1);
            });

            t1.Start();
            t2.Start();
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();


            Console.WriteLine($"Total count: {c1.Count}");

        }

        public void MutexConstruct()
        {
            const string MutexName = "CSharpThreadingCookbook";

            // Best pracice is to close the mutex property. 
            // wrapping in a using  helps release resource
            using(var m = new Mutex(false, MutexName))
            {
                if (!m.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    Console.WriteLine("Second instance is running!");
                }
                else
                {
                    Console.WriteLine("Running!");
                    Console.ReadLine();
                    m.ReleaseMutex();
                }
            }
        }
        public void SemaphoreSlimConstruct()
        {
            // Creating 6 threads
            for (int i = 1; i <=6; i++)
            {
                string threadName = $"Thread  {i}";
                int secondsToWait = 2 + (2 * i);
                var t = new Thread( () => {
                    AccessDataBase(threadName, secondsToWait);
                });
                t.Start();
            }
        }

        public void AutoResetEvent()
        {
            var t = new Thread( () => {
                Process(10);
            });
            t.Start();

            Console.WriteLine("Waiting for another thread to complete work");
            _workerEvent.WaitOne();
            Console.WriteLine("First operation is complete");
            Console.WriteLine("Performing operation on main thread");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            _mainEvent.Set();
            Console.WriteLine("Now running the second operation on a second thread");
            _workerEvent.WaitOne();
            Console.WriteLine("Second opeation is completed!");

        }

        public void ManualResetEventSlim()
        {
            var t1 = new Thread( () => {
                TravelThroughGates("Thread 1", 5);
            });
            var t2 = new Thread(() => {
                TravelThroughGates("Thread 2", 6);
            });
            var t3 = new Thread(() => {
                TravelThroughGates("Thread 3", 12);
            });

            t1.Start();
            t2.Start();
            t3.Start();

            Thread.Sleep(TimeSpan.FromSeconds(6));
             Console.WriteLine("The gates are now open!");
            _mainEventSlim.Set();  /// Opens the gates... thread 1 and thread 2 allowed in

            Thread.Sleep(TimeSpan.FromSeconds(2));
            _mainEventSlim.Reset();  // Close the gates

            Console.WriteLine("The gates are now closed!");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            Console.WriteLine("The gates are now open for the second time!");
            _mainEventSlim.Set();   // Opens the gates, thread 3 now allowed access

            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine("The gates have been closed");
            _mainEventSlim.Reset();  // close the gates

        }
    }
}
