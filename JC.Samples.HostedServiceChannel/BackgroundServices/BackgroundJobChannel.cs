using JC.Samples.HostedServiceChannel.BackgroundServices.Interfaces;
using JC.Samples.HostedServiceChannel.Models;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace JC.Samples.HostedServiceChannel.BackgroundServices;

/// <summary>
/// The Background Job Channel.
/// Wraps a <see cref="Channel"/>.
/// </summary>
public class BackgroundJobChannel : IBackgroundJobChannel
{
    private const int MaxChannelMessages = 100;

    private readonly ILogger<BackgroundJobChannel> _logger;
    private readonly Channel<BackgroundJobMessage> _channel;
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _jobsInProgress = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/></param>
    public BackgroundJobChannel(ILogger<BackgroundJobChannel> logger)
    {
        _logger = logger;

        // Limit the capacity of the channel queue.
        var options = new BoundedChannelOptions(MaxChannelMessages)
        {
            SingleWriter = false, // Multiple producers.
            SingleReader = true   // Single consumer.
        };

        _channel = Channel.CreateBounded<BackgroundJobMessage>(options);
    }

    /// <summary>
    /// Adds a job to the channel queue for processing.
    /// </summary>
    /// <param name="message">The job message to queue</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if the job was added to the queue successfully, otherwise false</returns>
    public async Task<bool> AddJobAsync(BackgroundJobMessage message, CancellationToken cancellationToken)
    {
        while (await _channel.Writer.WaitToWriteAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
        {
            if (_channel.Writer.TryWrite(message) && _jobsInProgress.TryAdd(message.JobId, message.CancellationTokenSource))
            {
                _logger.LogInformation("Job {0} has been queued", message.JobId);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Cancels a job that is currently in progress.
    /// </summary>
    /// <param name="jobId">The Job ID to cancel</param>
    /// <returns>True if the job was cancelled succesfully, otherwise false</returns>
    public bool CancelJob(Guid jobId)
    {
        if (_jobsInProgress.TryRemove(jobId, out var job) && job is not null)
        {
            job.Cancel();
            job.Dispose();

            _logger.LogInformation("Job {0} has been cancelled", jobId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Marks a job that is currently in progress as completed.
    /// </summary>
    /// <param name="jobId">The Job ID to mark as completed</param>
    /// <returns>True if the job was marked as completed succesfully, otherwise false</returns>
    public bool CompleteJob(Guid jobId)
    {
        if (_jobsInProgress.TryRemove(jobId, out var job))
        {
            job?.Dispose();

            _logger.LogInformation("Job {0} has been completed", jobId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Reads all messages from the channel queue.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>The job messages that are currently on the queue</returns>
    public IAsyncEnumerable<BackgroundJobMessage> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
