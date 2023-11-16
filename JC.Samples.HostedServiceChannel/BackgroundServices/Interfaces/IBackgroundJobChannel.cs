using JC.Samples.HostedServiceChannel.Models;

namespace JC.Samples.HostedServiceChannel.BackgroundServices.Interfaces;

/// <summary>
/// Background Job Channel interface.
/// </summary>
public interface IBackgroundJobChannel
{
    Task<bool> AddJobAsync(BackgroundJobMessage message, CancellationToken cancellationToken);
    bool CancelJob(Guid jobId);
    bool CompleteJob(Guid jobId);
    IAsyncEnumerable<BackgroundJobMessage> ReadAllAsync(CancellationToken cancellationToken);
}