using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace Chapter8_ReactiveExtensions
{
    class CustomSequence : IObservable<int>
    {
        private readonly IEnumerable<int> _numbers;
         

        public CustomSequence(IEnumerable<int> numbers)
        {
            _numbers = numbers;
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            foreach (var number in _numbers)
            {
                observer.OnNext(number);
            }

            observer.OnCompleted();
            return Disposable.Empty;
        }

       
    }
}
