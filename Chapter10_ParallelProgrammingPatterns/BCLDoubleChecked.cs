using System.Threading;

namespace Chapter10_ParallelProgrammingPatterns
{
    internal class BCLDoubleChecked : IHasValue
    {
        private object _syncRoot = new object();
        private ValueToAccess _value;
        private bool _initialized;
        

        public ValueToAccess Value => LazyInitializer.EnsureInitialized(ref _value, ref _initialized, ref _syncRoot, ValueToAccess.Compute);
    }
}