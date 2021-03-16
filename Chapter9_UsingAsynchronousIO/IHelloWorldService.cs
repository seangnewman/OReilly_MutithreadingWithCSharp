using System.ServiceModel;
using System.Threading.Tasks;

namespace Chapter9_UsingAsynchronousIO
{
   
    [ServiceContract(Namespace = "Chapter9_UsingAsynchronousIO", Name = "HelloWorldServiceContract")]
    internal interface IHelloWorldService
    {
        [OperationContract]
        string Greet(string name);

    }
}