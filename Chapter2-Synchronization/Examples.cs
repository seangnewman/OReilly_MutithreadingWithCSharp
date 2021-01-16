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
        static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(4);

        private static readonly AutoResetEvent _workerEvent = new AutoResetEvent(false);
        private static readonly AutoResetEvent _mainEvent = new AutoResetEvent(false);

        private static readonly ManualResetEventSlim _mainEventSlim = new ManualResetEventSlim(false);
        private static readonly CountdownEvent _countdown = new CountdownEvent(2);
        private static readonly Barrier _barrier = new Barrier(2, b => Console.WriteLine(b.CurrentPhaseNumber + 1));
        private static readonly ReaderWriterLockSlim _rw = new ReaderWriterLockSlim();
        private static readonly Dictionary<int, int> _items = new Dictionary<int, int>();
        // Use volatile to indicate a value may be modied by multiple threads
        // compiler will not optimize, and ensures the most up to date value is present
        private static volatile bool _isCompleted = false;


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

        static void PerformOperation(string message, int seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine(message);
            _countdown.Signal();
        }

        static void TravelThroughGates(string threadName, int seconds)
        {
            Console.WriteLine($"{threadName} falls to sleep");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine($"{threadName} waits for the gates to open!");
            _mainEventSlim.Wait();
            Console.WriteLine($"{threadName} enters the gates!");
        }
        static void PlayMusic(string name, string message, int seconds)
        {
            for (int i = 1; i < 3; i++)
            {
                Console.WriteLine("------------------------------------------------");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                Console.WriteLine($"{name} starts to {message}");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                Console.WriteLine($"{name} finishes to {message}");
                _barrier.SignalAndWait();
            }
        }

        static void Read()
        {
            Console.WriteLine("Reading contents of a dictionary");
            while (true)
            {
                try
                {
                    _rw.EnterReadLock();
                    foreach (var key in _items.Keys)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                    }

                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    _rw.ExitReadLock();
                }
            }
        }

        static void Write(string threadName)
        {
            while (true)
            {
                try
                {
                    int newKey = new Random().Next(250);
                    _rw.EnterUpgradeableReadLock();
                    if (!_items.ContainsKey(newKey))
                    {
                        try
                        {
                            _rw.EnterWriteLock();
                            _items[newKey] = 1;
                            Console.WriteLine($"New key {newKey} is added to a dictinary by a {threadName}");
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                        finally
                        {
                            _rw.ExitWriteLock();
                        }
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(0.1));
                }

                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    _rw.ExitUpgradeableReadLock();
                }
            }
        }

        static void UserModeWait()
        {
            while (!_isCompleted)
            {
                Console.WriteLine(".");
            }
            Console.WriteLine();
            Console.WriteLine("Waiting is complete");
        }

        static void HybridSpinWait()
        {
            var w = new SpinWait();
            while (!_isCompleted)
            {
                w.SpinOnce();
                Console.WriteLine(w.NextSpinWillYield);
            }
            Console.WriteLine("Waiting is complete");
        }
        // ----------------------------------------------
        public void BasicAtomicOperations()
        {
            Console.WriteLine("Increment counter");
            var c = new Counter();

            var t1 = new Thread(() =>
            {
                TestCounter(c);
            });
            var t2 = new Thread(() =>
            {
                TestCounter(c);
            });
            var t3 = new Thread(() =>
            {
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

            t1 = new Thread(() =>
            {
                TestCounter(c1);
            });
            t2 = new Thread(() =>
            {
                TestCounter(c1);
            });
            t3 = new Thread(() =>
            {
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
            using (var m = new Mutex(false, MutexName))
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
            for (int i = 1; i <= 6; i++)
            {
                string threadName = $"Thread  {i}";
                int secondsToWait = 2 + (2 * i);
                var t = new Thread(() =>
                {
                    AccessDataBase(threadName, secondsToWait);
                });
                t.Start();
            }
        }

        public void AutoResetEvent()
        {
            var t = new Thread(() =>
            {
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
            var t1 = new Thread(() =>
            {
                TravelThroughGates("Thread 1", 5);
            });
            var t2 = new Thread(() =>
            {
                TravelThroughGates("Thread 2", 6);
            });
            var t3 = new Thread(() =>
            {
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

        public void CountDownEvent()
        {
            Console.WriteLine("Starting two operations");
            var t1 = new Thread(() =>
            {
                PerformOperation("Operation 1 is completed", 4);
            });
            var t2 = new Thread(() =>
            {
                PerformOperation("Operation 2 is completed", 8);
            });

            t1.Start();
            t2.Start();

            _countdown.Wait();   // Will wait.. possibly forever... until all signals are received
            Console.WriteLine("Both operations have been completed.");
            _countdown.Dispose();

        }

        public void BarrierConstruct()
        {
            var t1 = new Thread(() =>
            {
                PlayMusic("the guitarist", "play an amazing solo", 5);
            });
            var t2 = new Thread(() =>
            {
                PlayMusic("the singer", "sing his song", 2);
            });

            t1.Start();
            t2.Start();
        }

        public void ReaderWriterSlimConstruct()
        {
            //ReaderWriterSlim insures thread safety despite 5 concurrent threads
            new Thread(Read) { IsBackground = true }.Start();
            new Thread(Read) { IsBackground = true }.Start();
            new Thread(Read) { IsBackground = true }.Start();

            new Thread(() => Write("Thread 1")) { IsBackground = true }.Start();
            new Thread(() => Write("Thread 2")) { IsBackground = true }.Start();

            Thread.Sleep(TimeSpan.FromSeconds(20));
        }

        public void SpinWaitConstruct()
        {

            //SpinWait  saves CPU time

            var t1 = new Thread(UserModeWait);
            var t2 = new Thread(HybridSpinWait);

            Console.WriteLine("Running user mode waiting");
            t1.Start();
            Thread.Sleep(20);
            _isCompleted = true;

            Thread.Sleep(TimeSpan.FromSeconds(1));
            _isCompleted = false;
            Console.WriteLine("Running hybrid SpinWait construct waiting");
            t2.Start();
            Thread.Sleep(5);

            _isCompleted = true;

        }
    }
}
