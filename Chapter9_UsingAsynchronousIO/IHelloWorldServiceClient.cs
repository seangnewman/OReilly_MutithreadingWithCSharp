using System.ServiceModel;
using System.Threading.Tasks;

namespace Chapter9_UsingAsynchronousIO
{
    [ServiceContract(Namespace = "Chapter9_UsingAsynchronousIO", Name = "HelloWorldServiceContract")]
    interface IHelloWorldServiceClient
    {
        [OperationContract]
        string Greet(string name);

        [OperationContract]
        Task<string> GreetAsync(string name);
    }
}
