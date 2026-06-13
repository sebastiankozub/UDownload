using Microsoft.Extensions.DependencyInjection;

namespace UtubeRest.Service;

public sealed class StreamDownloadBackgroundService : BackgroundService
{
    private readonly IStreamDownloadQueue _streamDownloadQueue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<StreamDownloadBackgroundService> _logger;

    public StreamDownloadBackgroundService(
        IStreamDownloadQueue streamDownloadQueue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<StreamDownloadBackgroundService> logger)
    {
        _streamDownloadQueue = streamDownloadQueue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            QueuedStreamDownloadRequest request;

            try
            {
                request = await _streamDownloadQueue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var ytService = scope.ServiceProvider.GetRequiredService<YtService>();

                _logger.LogInformation(
                    "Starting queued download {JobId} for {VideoId}.",
                    request.JobId,
                    request.VideoId);

                await ytService.DownloadBestAudioVideoAsync(request.VideoId, stoppingToken);

                _logger.LogInformation(
                    "Completed queued download {JobId} for {VideoId}.",
                    request.JobId,
                    request.VideoId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Queued download {JobId} failed for {VideoId}. Selected audio hash {AudioHashId}, video hash {VideoHashId}.",
                    request.JobId,
                    request.VideoId,
                    request.AudioHashId,
                    request.VideoHashId);
            }
        }
    }
}
