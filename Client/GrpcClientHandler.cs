using Grpc.Core;
using Grpc.Net.Client;

namespace Dionysus.Client;

public class GrpcClientHandler<TClient> where TClient : ClientBase
{
    private GrpcClientHandler(TClient client)
    {
        Client = client;
    }

    public static async Task<GrpcClientHandler<TClient>> Create(string address, Func<GrpcChannel, TClient> constructor)
    {
        var channel = await GrpcChannelPool.Get(address).ConfigureAwait(false);
        return new GrpcClientHandler<TClient>(constructor(channel));
    }

    public TClient Client { get; }
}
