namespace Chapter10_ParallelProgrammingPatterns
{
    internal class UnsafeState : IHasValue
    {
         
         private ValueToAccess _value;
        public ValueToAccess Value => _value ?? ( _value = ValueToAccess.Compute() );
        
    }
}