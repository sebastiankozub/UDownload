using System.Threading.Channels;

namespace UtubeRest.Service;

public sealed record QueuedStreamDownloadRequest(
    string JobId,
    string VideoId,
    string AudioFormatId,
    string VideoFormatId);

public interface IStreamDownloadQueue
{
    ValueTask QueueAsync(QueuedStreamDownloadRequest request, CancellationToken cancellationToken = default);
    ValueTask<QueuedStreamDownloadRequest> DequeueAsync(CancellationToken cancellationToken);
}

public sealed class StreamDownloadQueue : IStreamDownloadQueue
{
    private readonly Channel<QueuedStreamDownloadRequest> _queue = Channel.CreateUnbounded<QueuedStreamDownloadRequest>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });

    public ValueTask QueueAsync(QueuedStreamDownloadRequest request, CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(request, cancellationToken);
    }

    public ValueTask<QueuedStreamDownloadRequest> DequeueAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}
