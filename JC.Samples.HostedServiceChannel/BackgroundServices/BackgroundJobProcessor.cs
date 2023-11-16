using JC.Samples.HostedServiceChannel.BackgroundServices.Interfaces;
using JC.Samples.HostedServiceChannel.Models;

namespace JC.Samples.HostedServiceChannel.BackgroundServices;

/// <summary>
/// Processes background jobs.
/// </summary>
public class BackgroundJobProcessor : IBackgroundJobProcessor
{
    private readonly ILogger<BackgroundJobProcessor> _logger;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/></param>
    public BackgroundJobProcessor(ILogger<BackgroundJobProcessor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Processes the specified job message.
    /// </summary>
    /// <param name="message">The job message to process</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"/></returns>
    public async Task ProcessAsync(BackgroundJobMessage message, CancellationToken cancellationToken)
    {
        if (message.Data == null)
        {
            _logger.LogWarning("Data is null");
            return;
        }

        _logger.LogDebug("Processing job {0}. Data: {1}", message.JobId, message.Data);

        _logger.LogDebug("Replacing characters");
        message.Data = message.Data.Replace("a", "b");

        await Task.Delay(3000);

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogDebug("Capitalizing characters");
        message.Data = message.Data.ToUpper();

        await Task.Delay(6000);

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogDebug("Trimming spaces");
        message.Data = message.Data.Trim();

        await Task.Delay(9000);

        cancellationToken.ThrowIfCancellationRequested();
    }
}
