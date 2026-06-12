using System.Collections.Concurrent;
using Grpc.Net.Client;

namespace Dionysus.Client;

public static class GrpcChannelPool
{
    static GrpcChannelPool()
    {
        MonitoringThread.Start();
    }

    public static async Task<GrpcChannel> Get(string address)
    {
        while (MonitoringSemaphore.CurrentCount < 1)
        {
            await Task.Delay(100).ConfigureAwait(false);
        }

        if (Channels.TryGetValue(address, out var channelMember))
            return channelMember.Channel;

        channelMember = new GrpcChannelPoolMember(address);
        Channels.TryAdd(address, channelMember);

        return channelMember.Channel;
    }

    private static void MonitoringLoop()
    {
        while (!CancellationToken.IsCancellationRequested)
        {
            Thread.Sleep(MonitoringInterval);

            MonitoringSemaphore.Wait();

            foreach (var (address, channelMember) in Channels)
            {
                if (!channelMember.IsExpired)
                    continue;

                Channels.TryRemove(address, out _);
                channelMember.Channel.Dispose();
            }
        }
    }

    private static readonly CancellationToken CancellationToken = CancellationToken.None;
    private static readonly ConcurrentDictionary<string, GrpcChannelPoolMember> Channels = [];
    private const int MonitoringInterval = 1000 * 60 * 15;
    private static readonly SemaphoreSlim MonitoringSemaphore = new(1, 1);
    private static readonly Thread MonitoringThread = new(MonitoringLoop);
}

public class GrpcChannelPoolMember(string address, int lifetime = 15, LifetimeType lifetimeType = LifetimeType.Minutes)
{
    public GrpcChannel Channel { get; } = GrpcChannel.ForAddress(address);
    public bool IsExpired => DateTime.UtcNow > _expiryTime;

    private readonly DateTime _expiryTime = DateTime.UtcNow.AddLifetime(lifetime, lifetimeType);
}

public static class DateTimeLifetimeExtensions
{
    public static DateTime AddLifetime(this DateTime dateTime, int lifetime, LifetimeType lifetimeType)
    {
        return lifetimeType switch
        {
            LifetimeType.Seconds => dateTime.AddSeconds(lifetime),
            LifetimeType.Minutes => dateTime.AddMinutes(lifetime),
            LifetimeType.Hours => dateTime.AddHours(lifetime),
            LifetimeType.Days => dateTime.AddDays(lifetime),
            _ => throw new ArgumentOutOfRangeException(nameof(lifetimeType), lifetimeType, null)
        };
    }
}

public enum LifetimeType
{
    Seconds,
    Minutes,
    Hours,
    Days
}
