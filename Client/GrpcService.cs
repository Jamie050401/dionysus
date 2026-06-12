using Grpc.Net.Client;

namespace Dionysus.Client;

// TODO: Consider possible generic type constraints for TClient
public class GrpcService<TClient> : IDisposable
{
    public GrpcService(string address, Func<GrpcChannel, TClient> constructor)
    {
        _channel = GrpcChannel.ForAddress(address);
        Client = constructor(_channel);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (_isDisposed)
            return;

        if (isDisposing)
        {
            _channel.Dispose();
        }

        _isDisposed = true;
    }

    public TClient Client { get; }
    
    private readonly GrpcChannel _channel;
    private bool _isDisposed = false;
}