using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter10_ParallelProgrammingPatterns
{
    class PipelineWorker<TInput, TOutput>
    {
        Func<TInput, TOutput> _processor;
        Action<TInput> _outputProcessor;
        BlockingCollection<TInput>[] _input;
        CancellationToken _token;
        Random _rnd;

        public PipelineWorker(
            BlockingCollection<TInput>[] input,
            Func<TInput, TOutput> processor,
            CancellationToken token,
            string name,
            int Count=5)
        {
            _input = input;
            Output = new BlockingCollection<TOutput>[_input.Length];
            for (int i = 0; i < Output.Length; i++)
                Output[i] = null == input[i] ? null
                  : new BlockingCollection<TOutput>(Count);

            _processor = processor;
            _token = token;
            Name = name;
            _rnd = new Random(DateTime.Now.Millisecond);
        }

        public PipelineWorker(
            BlockingCollection<TInput>[] input,
            Action<TInput> renderer,
            CancellationToken token,
            string name)
        {
            _input = input;
            _outputProcessor = renderer;
            _token = token;
            Name = name;
            Output = null;
            _rnd = new Random(DateTime.Now.Millisecond);
        }

        public BlockingCollection<TOutput>[] Output { get; private set; }

        public string Name { get; private set; }

        public void Run()
        {
            Console.WriteLine($"{Name} is running");
            while (!_input.All(bc => bc.IsCompleted) &&
              !_token.IsCancellationRequested)
            {
                TInput receivedItem;
                int i = BlockingCollection<TInput>.TryTakeFromAny(
                    _input, out receivedItem, 50, _token);
                if (i >= 0)
                {
                    if (Output != null)
                    {
                        TOutput outputItem = _processor(receivedItem);
                        BlockingCollection<TOutput>.AddToAny(
                            Output, outputItem);
                        Console.WriteLine($"{Name} sent {outputItem} to next, on " +
                        $"thread id {Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(_rnd.Next(200)));
                    }
                    else
                    {
                        _outputProcessor(receivedItem);
                    }
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(50));
                }
            }
            if (Output != null)
            {
                foreach (var bc in Output) bc.CompleteAdding();
            }
        }

    }

}
