using JC.Samples.HostedServiceChannel.BackgroundServices.Interfaces;
using JC.Samples.HostedServiceChannel.Models;

namespace JC.Samples.HostedServiceChannel.BackgroundServices;

/// <summary>
/// Background service for processing jobs.
/// </summary>
public class BackgroundJobService : BackgroundService
{
    private readonly ILogger<BackgroundJobService> _logger;
    private readonly IBackgroundJobChannel _channel;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/></param>
    /// <param name="channel">The background job channel</param>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/></param>
    public BackgroundJobService(
        ILogger<BackgroundJobService> logger,
        IBackgroundJobChannel channel,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _channel = channel;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Executes when the service has started.
    /// </summary>
    /// <param name="stoppingToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"/></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Process messages from the channel queue asynchronously.
        await foreach (BackgroundJobMessage message in _channel.ReadAllAsync(stoppingToken))
        {
            // Process one at time.
            await ProcessJob(message, stoppingToken);

            // Process in the background.
            //_ = ProcessTask(message, stoppingToken);
        }
    }

    private async Task ProcessJob(BackgroundJobMessage message, CancellationToken stoppingToken)
    {
        try
        {
            // Associate a new cancellation token source with the job cancellation token.
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, message.CancellationTokenSource.Token);

            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            _logger.LogInformation("Processing job {0}", message.JobId);

            // Since the Background Service is registered as a Singleton in the DI container and
            // the task processor is a Scoped service, we need to control its lifetime manually.
            using var scope = _serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IBackgroundJobProcessor>();

            // Run the job processing logic.
            await processor.ProcessAsync(message, cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
        finally
        {
            _channel.CompleteJob(message.JobId);
        }
    }
}
