namespace JC.Samples.HostedServiceChannel.Models;

/// <summary>
/// The background job response returned by the API.
/// </summary>
/// <param name="JobId">The unique Job ID</param>
/// <param name="IsCompleted">Whether or not the job is completed</param>
/// <param name="Data">The processed data</param>
public record BackgroundJobResponse(Guid JobId, bool IsCompleted, string? Data = null);