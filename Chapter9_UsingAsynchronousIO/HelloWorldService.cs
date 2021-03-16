using System.Threading.Tasks;

namespace Chapter9_UsingAsynchronousIO
{
    internal class HelloWorldService : IHelloWorldService
    {
        public string Greet(string name)
        {
            return $"Greetings, {name}";
        }

    }

}