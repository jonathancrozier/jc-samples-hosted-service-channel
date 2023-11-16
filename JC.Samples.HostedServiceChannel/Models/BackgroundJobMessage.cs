namespace JC.Samples.HostedServiceChannel.Models;

/// <summary>
/// The background job message that is used when processing the job.
/// </summary>
public class BackgroundJobMessage
{
    /// <summary>
    /// The unique Job ID.
    /// </summary>
    public Guid JobId { get; } = Guid.NewGuid();

    /// <summary>
    /// The data to be processed.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// <see cref="CancellationTokenSource"/>.
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="data">The data to be processed</param>
    /// <param name="cancellationTokenSource"><see cref="CancellationTokenSource"/></param>
    public BackgroundJobMessage(string data, CancellationTokenSource cancellationTokenSource)
    {
        Data = data;
        CancellationTokenSource = cancellationTokenSource;
    }
}