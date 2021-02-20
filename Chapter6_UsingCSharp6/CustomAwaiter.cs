using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Chapter6_UsingCSharp6
{
    public class CustomAwaiter : INotifyCompletion
    {
        private string  _result = "Completed synchronously";
        private readonly bool _completeSynchronously;
        public bool IsCompleted => _completeSynchronously;



        public CustomAwaiter(bool completeSynchronously)
        {
            _completeSynchronously = completeSynchronously;
        }

        public string GetResult()
        {
            return _result;
        }

        public void OnCompleted(Action continuation)
        {
            ThreadPool.QueueUserWorkItem( state => {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                _result = GetInfo();
                continuation?.Invoke();

            });
        }

        private string GetInfo()
        {
            return $"Task is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread? {Thread.CurrentThread.IsThreadPoolThread}";
        }
    }
}