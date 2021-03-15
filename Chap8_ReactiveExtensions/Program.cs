using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter8_ReactiveExtensions
{
    class Program
    {
        static void Main(string[] args)
        {
            Examples example = new Examples();
            //example.ConvertingCollectionToAsynchronousObservable();
            //example.WritingCustomObservable();
            //example.UsingSubectTypeFamily();
            //example.CreatingAnObservableObject();
            //example.UsingLINQQueriesAgainstObservableCollection();
            example.CreatingAsynchronousOperationsWithRx();
        }

    }
}
