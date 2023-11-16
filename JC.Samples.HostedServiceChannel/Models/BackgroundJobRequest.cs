namespace JC.Samples.HostedServiceChannel.Models;

/// <summary>
/// The background job request passed to the API.
/// </summary>
/// <param name="Data">The data to be processed</param>
public record BackgroundJobRequest(string Data);