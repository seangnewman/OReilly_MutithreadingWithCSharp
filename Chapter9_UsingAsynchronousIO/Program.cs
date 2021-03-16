using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter9_UsingAsynchronousIO
{
    class Program
    {
        static void Main(string[] args)
        {
            Examples example = new Examples();

            //example.WorkingWithFilesAsynchronously();
            //example.WritingAsynchronousHTTPServerClient();
            //example.WorkingWithDatabaseAsynchronously();
            example.CallingAWCFServiceAsynchronously();    /// Must be run with admin privilidge
            
        }
    }
}
