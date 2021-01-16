using System;
using System.Threading;

using static System.Diagnostics.Process;


namespace Chapter1
{
    public class Examples
    {

        public static void Count(object iterations)
        {
            CountNumbers((int)iterations);
        }

        private static void CountNumbers(int iterations)
        {
            for (int i = 1; i < iterations; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine($"{Thread.CurrentThread.Name} prints {i}");
            }
        }

        private static void PrintNumber(int number)
        {
            Console.WriteLine(number);
        }
        static void PrintNumbers()
        {
            Console.WriteLine("Starting....");
            for (int i = 1; i < 10; i++)
            {
                Console.WriteLine(i);
            }
        }

        static void PrintNumbersWithDelay()
        {
            Console.WriteLine("Starting....");
            for (int i = 1; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine(i);
            }
        }

        static void PrintNumbersWithStatus()
        {
            Console.WriteLine("Starting......");
            Console.WriteLine(Thread.CurrentThread.ThreadState.ToString());
            for (int i = 1; i < 10; i++)
            {
                //While running, status changes to WaitSleepJoin
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine(i);
            }
        }

        static void DoNothing()
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        static void TestCounter(CounterBase c)
        {
            for (int i = 0; i < 100000; i++)
            {
                c.Increment();
                c.Decrement();
            }
        }

        static void RunThreads()
        {
            var sample = new ThreadSample();

            var threadOne = new Thread(sample.CountNumbers)
            {
                Name = "ThreadOne"
            };
            var threadTwo = new Thread(sample.CountNumbers)
            {
                Name = "ThreadTwo"
            };

            threadOne.Priority = System.Threading.ThreadPriority.Highest;
            threadTwo.Priority = System.Threading.ThreadPriority.Lowest;

            threadOne.Start();
            threadTwo.Start();

            Thread.Sleep(TimeSpan.FromSeconds(2));
            sample.Stop();
        }

        static void LockTooMuch(object lock1, object lock2)
        {
            lock (lock1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                lock (lock2) ;
            }
        }

        static void BadFaultyThread()
        {
            Console.WriteLine("Starting a faulty thread...");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            throw new Exception("Boom!");
        }

        static void FaultyThread()
        {
            try
            {
                Console.WriteLine("Starting a faulty thread.....");
                Thread.Sleep(TimeSpan.FromSeconds(1));
                throw new Exception("Boom!");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Exception Handled ...{ex.Message}");
            }
        }
        // *****  Examples

        public void CreateAThread()
        {
            Thread t = new Thread(PrintNumbers);
            t.Start();
            PrintNumbers();
        }

        public void MakingAThreadWait()
        {
            Console.WriteLine("Starting....");
            Thread t = new Thread(PrintNumbersWithDelay);
            t.Start();
            t.Join();   // Directs the main thread to wait until the background task has completed
            Console.WriteLine("Thread Completed");
        }

        public void AbortAThread()
        {
            Console.WriteLine("Starting program ... ");
            Thread t = new Thread(PrintNumbersWithDelay);
            t.Start();
            Thread.Sleep(TimeSpan.FromSeconds(6));

            // Using Abort on a thread is very dangerous!
            //
            t.Abort();

            Console.WriteLine("A thread has been aborted");
            t.Start();
            PrintNumbers();
        }

        public void ThreadState()
        {
            Console.WriteLine("Starting program ...");
            Thread t = new Thread(PrintNumbersWithStatus);
            Thread t2 = new Thread(DoNothing);
            Console.WriteLine(t.ThreadState.ToString());

            t2.Start();
            t.Start();

            for (int i = 1; i < 300; i++)
            {
                Console.WriteLine(t.ThreadState.ToString());
            }

            Thread.Sleep(TimeSpan.FromSeconds(6));

            t.Abort();
            Console.WriteLine("A thread has been aborted...");
            Console.WriteLine(t.ThreadState.ToString());
            Console.WriteLine(t2.ThreadState.ToString());
        }

        public void ThreadPriority()
        {
            Console.WriteLine($"Current thread priority: {Thread.CurrentThread.Priority}");
            Console.WriteLine("Running on all available cores");
            RunThreads();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine("Running on a single core");
            GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
            RunThreads();

        }

        public void ForeGroundAndBackgroundThreads()
        {
            var sampleForeground = new ThreadSample(30);
            var sampleBackground = new ThreadSample(100);

            var threadOne = new Thread(sampleForeground.CountNumbers)
            {
                Name = "ForegroundThread"
            };
            var threadTwo = new Thread(sampleBackground.CountNumbers)
            {
                Name = "BackgroundThread",
                IsBackground = true
            };

            threadOne.Start();
            threadTwo.Start();




        }

        public void PassingThreadParameters()
        {
            var sample = new ThreadSample(10);

            var threadOne = new Thread(sample.CountNumbers)
            {
                Name = "ThreadOne"
            };
            threadOne.Start();
            threadOne.Join();
            Console.WriteLine("-------------------------------");

            var threadTwo = new Thread(Count)
            {
                Name = "ThreadTwo"
            };
            threadTwo.Start(8);
            threadTwo.Join();
            Console.WriteLine("-------------------------------");

            var threadThree = new Thread(() => CountNumbers(12))
            {
                Name = "ThreadThreee"
            };

            threadThree.Start();
            threadThree.Join();
            Console.WriteLine("-------------------------------");

            int i = 10;
            var threadFour = new Thread(() => PrintNumber(i));

            i = 20;
            var threadFive = new Thread(() => PrintNumber(i));
            threadFour.Start();
            threadFive.Start();






        }

        public void ThreadUsingLocks()
        {
            Console.WriteLine("Incorrect counter");

            var c = new Counter();

            var t1 = new Thread(() => TestCounter(c));
            var t2 = new Thread(() => TestCounter(c));
            var t3 = new Thread(() => TestCounter(c));

            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

            Console.WriteLine($"Total count: {c.Count}");
            Console.WriteLine("-----------------------------");

            Console.WriteLine("Correct counter");

            var c1 = new CounterWithLock();

            t1 = new Thread(() => TestCounter(c1));
            t2 = new Thread(() => TestCounter(c1));
            t3 = new Thread(() => TestCounter(c1));

            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

            Console.WriteLine($"Total count: {c1.Count}");



        }

        public void ThreadLocksWithMonitors()
        {
            object lock1 = new object();
            object lock2 = new object();

            new Thread(() => LockTooMuch(lock1, lock2)).Start();

            lock (lock2)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Monitor.TryEnter allows not to get stuck, returning false after a specified  amount of time");
                if (Monitor.TryEnter(lock1, TimeSpan.FromSeconds(1)))
                {
                    Console.WriteLine("Acquired a protected resource successfully");
                }
                else
                {
                    Console.WriteLine("Timeout acquiring a resource!");
                }
            }

            new Thread(() => LockTooMuch(lock1, lock2)).Start();

            Console.WriteLine("---------------------------------------------");

            lock (lock2)
            {

                Console.WriteLine("This will result in a deadlock");
                Thread.Sleep(1000);

                lock (lock1)
                {
                    Console.WriteLine("Acquired a protected resource successfully");
                }
            }


        }

        public void ThreadExceptions()
        {
            var t = new Thread(FaultyThread);
            t.Start();
            t.Join();

            try
            {
                t = new Thread(BadFaultyThread);
                t.Start();
            }
            catch (Exception)
            {

                Console.WriteLine("We won't get here!");
            }
        }

    }
}
