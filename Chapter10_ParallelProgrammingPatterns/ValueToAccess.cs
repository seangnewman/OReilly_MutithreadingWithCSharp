using System;
using System.Threading;

namespace Chapter10_ParallelProgrammingPatterns
{
    internal class ValueToAccess
    {
        private readonly string _text;
        public string Text => _text;

        public ValueToAccess(string text)
        {
            _text = text;
        }

        public static ValueToAccess Compute()
        {
            Console.WriteLine("The value is being constructed on a thread " +
                  $"id {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return new ValueToAccess(
                      $"Constructed on thread id {Thread.CurrentThread.ManagedThreadId}");
        }

    }

}