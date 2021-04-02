namespace Chapter10_ParallelProgrammingPatterns
{
    internal class DoubleCheckedLocking : IHasValue
    {
        private readonly object _syncRoot = new object();
        private volatile ValueToAccess _value;

        public ValueToAccess Value 
        {
            get 
            {
                if (_value == null)
                {
                    lock (_syncRoot)
                    {
                        if (_value == null)
                            _value = ValueToAccess.Compute();
                    }
                }
                return _value;
            }
        }
               
    }
}