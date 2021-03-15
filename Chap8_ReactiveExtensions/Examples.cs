using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Chapter8_ReactiveExtensions
{
    class Examples
    {
        delegate string AsyncDelegate(string name);
        static IEnumerable<int> EnumerableEventSequence()
        {
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                yield return i;
            }
        }

        static IDisposable OutputToConsole<T>(IObservable<T> sequence)
        {
            return sequence.Subscribe(obj => Console.WriteLine($"{obj}"),
                                                                      ex => Console.WriteLine($"Error: {ex.Message}"),
                                                                      () => Console.WriteLine("Completed"));
        }
        static IDisposable OutputToConsole<T>(IObservable<EventPattern<ElapsedEventArgs>> sequence)
        {
            return sequence.Subscribe(obj => Console.WriteLine($"{obj.EventArgs.SignalTime}"),
                                                                      ex => Console.WriteLine($"Error: {ex.Message}"),
                                                                      () => Console.WriteLine("Completed"));
        }

        static IDisposable OutputToConsole<T>(IObservable<T> sequence, int innerLevel)
        {
            string delimiter = innerLevel == 0
                  ? string.Empty
                  : new string('-', innerLevel * 3);

            return sequence.Subscribe(
              obj => Console.WriteLine($"{delimiter}{obj}")
              , ex => Console.WriteLine($"Error: {ex.Message}")
              , () => Console.WriteLine($"{delimiter}Completed")
            );
        }

        static async Task<T> AwaitOnObservable<T>(IObservable<T> observable)
        {
            T obj = await observable;
            Console.WriteLine($"{obj}");
            return obj;
        }

        static Task<string> LongRunningOperationTaskAsync(string name)
        {
            return Task.Run(() => LongRunningOperation(name));
        }

        static IObservable<string> LongRunningOperationAsync(string name)
        {
            return Observable.Start(() => LongRunningOperation(name));
        }
        private static string LongRunningOperation(string name)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return $"Task {name} is completed. Thread Id {Thread.CurrentThread.ManagedThreadId}";
        }

        // ************************
        public void ConvertingCollectionToAsynchronousObservable()
        {
            foreach (int i in EnumerableEventSequence())
            {
                Console.Write(i);
            }

            Console.WriteLine();
            Console.WriteLine("IEnumerable");

            IObservable<int> o = EnumerableEventSequence().ToObservable();

            using (IDisposable subscription = o.Subscribe(Console.Write))
            {
                Console.WriteLine();
                Console.WriteLine("IObservable");
            }

            o = EnumerableEventSequence().ToObservable().SubscribeOn(TaskPoolScheduler.Default);

            using (IDisposable subscription = o.Subscribe(Console.Write))
            {
                Console.WriteLine();
                Console.WriteLine("IObservable async");
                Console.ReadLine();
            }

        }

        public void WritingCustomObservable()
        {
            var observer = new CustomObserver();
            var goodObservable = new CustomSequence(new[] { 1, 2, 3, 4, 5 });
            var badObservable = new CustomSequence(null);

            using (IDisposable subscription = goodObservable.Subscribe(observer))
            {

            }

            using (IDisposable subscription = goodObservable.SubscribeOn(TaskPoolScheduler.Default).Subscribe(observer))
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                Console.WriteLine("Press ENTER to continue");
                Console.ReadLine();
            }

            using (IDisposable subscription = badObservable.SubscribeOn(TaskPoolScheduler.Default).Subscribe(observer))
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                Console.WriteLine("Press ENTER to continue");
                Console.ReadLine();
            }
        }

        public void UsingSubectTypeFamily()
        {
            Console.WriteLine("Subject");
            var subject = new Subject<string>();

            subject.OnNext("A");
            using (var subscription = OutputToConsole(subject))
            {
                subject.OnNext("B");
                subject.OnNext("C");
                subject.OnNext("D");
                subject.OnCompleted();
                subject.OnNext("Will not be printed out");
            }

            Console.WriteLine("ReplaySubject");
            var replaySubject = new ReplaySubject<string>();
            replaySubject.OnNext("A");
            using (var subscription = OutputToConsole(replaySubject))
            {
                replaySubject.OnNext("B");
                replaySubject.OnNext("C");
                replaySubject.OnNext("D");
                replaySubject.OnCompleted();
            }

            Console.WriteLine("Buffered ReplaySubject");
            var bufferedSubject = new ReplaySubject<string>();
            bufferedSubject.OnNext("A");
            bufferedSubject.OnNext("B");
            bufferedSubject.OnNext("C");
            using (var subscription = OutputToConsole(bufferedSubject))
            {
                bufferedSubject.OnNext("D");
                bufferedSubject.OnCompleted();
            }

            Console.WriteLine("Time window ReplaySubject");
            var timeSubject = new ReplaySubject<string>(TimeSpan.FromMilliseconds(200));
            timeSubject.OnNext("A");
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            timeSubject.OnNext("B");
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            timeSubject.OnNext("C");
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            using (var subscription = OutputToConsole(timeSubject))
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                timeSubject.OnNext("D");
                timeSubject.OnCompleted();
            }

            Console.WriteLine("AsyncSubject");
            var asyncSubject = new AsyncSubject<string>();
            asyncSubject.OnNext("A");
            using (var subscription = OutputToConsole(asyncSubject))
            {
                asyncSubject.OnNext("B");
                asyncSubject.OnNext("C");
                asyncSubject.OnNext("D");
                asyncSubject.OnCompleted();
            }

            Console.WriteLine("BehaviorSubject");
            var behaviorSubject = new BehaviorSubject<string>("Default");
            using (var subscription = OutputToConsole(behaviorSubject))
            {
                behaviorSubject.OnNext("A");
                behaviorSubject.OnNext("B");
                behaviorSubject.OnNext("C");
                behaviorSubject.OnNext("D");
                behaviorSubject.OnCompleted();
            }


        }

        public void CreatingAnObservableObject()
        {

            // Produces a single value
            IObservable<int> o = Observable.Return(0);
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine(" ---------------- ");

            // Produces no value
            o = Observable.Empty<int>();
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine(" ---------------- ");

            // Trigger the error handles  throws a System.Exception
            o = Observable.Throw<int>(new Exception());
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine(" ---------------- ");

            // Represents an endless sequence
            o = Observable.Repeat(42);
            using (var sub = OutputToConsole(o.Take(5))) ;
            Console.WriteLine(" ---------------- ");

            o = Observable.Range(0, 10);
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine(" ---------------- ");

            // Supporting custom scenarios using Observable.Create
            // Create the disposable object to represent a subscription
            o = Observable.Create<int>(ob =>
            {
                for (int i = 0; i < 10; i++)
                {
                    ob.OnNext(i);
                }
                return Disposable.Empty;
            });
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine(" ---------------- ");


            // An alternate way to create a custom sequence
            o = Observable.Generate(
              0 // initial state
              , i => i < 5 // while this is true we continue the sequence
              , i => ++i // iteration
              , i => i * 2 // selecting result
            );
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine(" ---------------- ");

            IObservable<long> ol = Observable.Interval(TimeSpan.FromSeconds(1));
            using (var sub = OutputToConsole(ol))
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
            };
            Console.WriteLine(" ---------------- ");

            ol = Observable.Timer(DateTimeOffset.Now.AddSeconds(2));
            using (var sub = OutputToConsole(ol))
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
            };
            Console.WriteLine(" ---------------- ");
        }

        public void UsingLINQQueriesAgainstObservableCollection()
        {
            IObservable<long> sequence = Observable.Interval(TimeSpan.FromMilliseconds(50)).Take(21);

            var evenNumbers = from n in sequence
                              where n % 2 == 0
                              select n;

            var oddNumbers = from n in sequence
                             where n % 2 != 0
                             select n;

            var combine = from n in evenNumbers.Concat(oddNumbers)
                          select n;

            var nums = (from n in combine
                        where n % 5 == 0
                        select n)
                .Do(n => Console.WriteLine($"------Number {n} is processed in Do method"));

            using (var sub = OutputToConsole(sequence, 0))
            using (var sub2 = OutputToConsole(combine, 1))
            using (var sub3 = OutputToConsole(nums, 2))
            {
                Console.WriteLine("Press enter to finish the demo");
                Console.ReadLine();
            }
        }

        public void CreatingAsynchronousOperationsWithRx()
        {
            IObservable<string> o = LongRunningOperationAsync("Task1");
            using (var sub = OutputToConsole(o))
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
            };
            Console.WriteLine(" ---------------- ");

            Task<string> t = LongRunningOperationTaskAsync("Task2");
            using (var sub = OutputToConsole(t.ToObservable()))
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
            };
            Console.WriteLine(" ---------------- ");

            AsyncDelegate asyncMethod = LongRunningOperation;

            // marked as obsolete, use tasks instead
            Func<string, IObservable<string>> observableFactory =
              Observable.FromAsyncPattern<string, string>(
                    asyncMethod.BeginInvoke, asyncMethod.EndInvoke);

            o = observableFactory("Task3");
            using (var sub = OutputToConsole(o))
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
            };
            Console.WriteLine(" ---------------- ");

            o = observableFactory("Task4");
            AwaitOnObservable(o).Wait();
            Console.WriteLine(" ---------------- ");

            using (var timer = new System.Timers.Timer(1000))
            {
                var ot = Observable.
                             FromEventPattern<ElapsedEventHandler, ElapsedEventArgs>(
                  h => timer.Elapsed += h,
                          h => timer.Elapsed -= h);
                timer.Start();

                using (var sub = OutputToConsole(ot))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
                Console.WriteLine(" ---------------- ");
                timer.Stop();
            }
        }
    }
}
