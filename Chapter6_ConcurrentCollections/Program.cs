
namespace Chapter6_ConcurrentCollections
{
    class Program
    {
        static void Main(string[] args)
        {
            var example = new Examples();

            //example.UsingConcurrentDictionary();
            //example.ImplementingAsyncProcessingUsingConcurrentQueue();
            example.ImplementingAsyncProcessingUsingConcurrentStack();
        }
    }
}
