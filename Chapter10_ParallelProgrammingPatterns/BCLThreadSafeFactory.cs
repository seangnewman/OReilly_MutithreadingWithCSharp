using System.Threading;

namespace Chapter10_ParallelProgrammingPatterns
{
    internal class BCLThreadSafeFactory : IHasValue
    {
        private ValueToAccess _value;

        public ValueToAccess Value =>  LazyInitializer.EnsureInitialized(ref _value, ValueToAccess.Compute);
    }
}