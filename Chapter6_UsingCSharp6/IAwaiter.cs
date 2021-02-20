using System.Runtime.CompilerServices;

namespace Chapter6_UsingCSharp6
{
    public  interface IAwaiter<T>: INotifyCompletion
    {
        bool IsCompleted { get; }

        T GetResult();

    }
}