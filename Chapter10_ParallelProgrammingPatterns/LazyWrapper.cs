using System;

namespace Chapter10_ParallelProgrammingPatterns
{
    internal class LazyWrapper : IHasValue
    {
        private readonly Lazy<ValueToAccess> _value;
         
        public LazyWrapper(Lazy<ValueToAccess> value)
        {
            _value = value;
        }

        public ValueToAccess Value => _value.Value;
    }
}