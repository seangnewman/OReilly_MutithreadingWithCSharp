namespace Chapter6_UsingCSharp6
{
    internal class CustomAwaitable
    {
         
        private readonly bool _completeSynchronously;

        public CustomAwaitable(bool completeSynchronously)
        {
            _completeSynchronously = completeSynchronously;
        }

        public CustomAwaiter GetAwaiter()
        {
            return new CustomAwaiter(_completeSynchronously);
        }

       
    }
}