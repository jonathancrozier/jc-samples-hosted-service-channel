using JC.Samples.HostedServiceChannel.BackgroundServices;
using JC.Samples.HostedServiceChannel.BackgroundServices.Interfaces;
using JC.Samples.HostedServiceChannel.Models;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IBackgroundJobProcessor, BackgroundJobProcessor>();
builder.Services.AddSingleton<IBackgroundJobChannel, BackgroundJobChannel>();
builder.Services.AddHostedService<BackgroundJobService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapPost("/backgroundJobs", async (
    BackgroundJobRequest request,
    IBackgroundJobChannel channel,
    CancellationToken cancellationToken,
    HttpContext httpContext) =>
{
    try
    {
        var message = new BackgroundJobMessage(request.Data, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));

        var addJobCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        addJobCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));

        try
        {
            var jobAdded = await channel.AddJobAsync(message, addJobCancellationTokenSource.Token);

            if (jobAdded)
            {
                var backgroundJobLink = new UriBuilder
                {
                    Scheme = httpContext.Request.Scheme,
                    Host = httpContext.Request.Host.Host,
                    Port = httpContext.Request.Host.Port.GetValueOrDefault(-1),
                    Path = $"/backgroundJobs/{message.JobId}"
                }.ToString();

                return TypedResults.Accepted(backgroundJobLink, new BackgroundJobResponse(message.JobId, false));
            }
        }
        catch (OperationCanceledException) when (addJobCancellationTokenSource.IsCancellationRequested)
        {
            return Results.StatusCode((int)HttpStatusCode.TooManyRequests);
        }

        return Results.BadRequest();
    }
    catch (Exception)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
});

app.MapDelete("/backgroundJobs/{jobId:guid}", (Guid jobId, IBackgroundJobChannel channel) =>
{
    try
    {
        bool cancelled = channel.CancelJob(jobId);

        if (cancelled)
        {
            return Results.NoContent();
        }

        return Results.NotFound();
    }
    catch
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
});

app.Run();