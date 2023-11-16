using JC.Samples.HostedServiceChannel.Models;

namespace JC.Samples.HostedServiceChannel.BackgroundServices.Interfaces;

/// <summary>
/// Background Job Processor interface.
/// </summary>
public interface IBackgroundJobProcessor
{
    Task ProcessAsync(BackgroundJobMessage message, CancellationToken cancellationToken);
}