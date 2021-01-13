using System;
using System.Threading;

namespace Chapter1
{
    public class ThreadSample
    {
        private bool _isStopped;
        private readonly int _iterations;

        public void Stop()
        {
            _isStopped = true;
        }

        public ThreadSample()
        {

        }
        public ThreadSample(int iterations)
        {
            _iterations = iterations;
        }

        //public void CountNumbers()
        //{
        //    long counter = 0;

        //    while (!_isStopped)
        //    {
        //        counter++;
        //    }

        //    Console.WriteLine($"{Thread.CurrentThread.Name} with $({Thread.CurrentThread.Priority,11} priority  has a count = ${counter, 13:N0}");
        //}

        public void CountNumbers()
        {
            for (int i = 0; i < _iterations; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine($"{Thread.CurrentThread.Name} prints {i}");
            }
        }
    }
}
