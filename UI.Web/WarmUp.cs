using Dionysus.Client;
using Dionysus.Server;

namespace Dionysus.UI.Web;

public static class WarmUp
{
    public static async Task Initialise()
    {
        var greeter = await GrpcClientHandler<Greeter.GreeterClient>
            .Create("127.0.0.1:0001", channel => new Greeter.GreeterClient(channel))
            .ConfigureAwait(false);

        await greeter.Client
            .SayHelloAsync(new HelloRequest { Name = "GreeterClient" })
            .ConfigureAwait(false);
    }
}
