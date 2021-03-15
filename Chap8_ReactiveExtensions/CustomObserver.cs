using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter8_ReactiveExtensions
{
    class CustomObserver: IObserver<int>
    {
        public void OnCompleted()
        {
            Console.WriteLine("Completed");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine($"Error: {error.Message}");
        }

        public void OnNext(int value)
        {
            Console.WriteLine($"Next value: {value}; Thread Id: {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
