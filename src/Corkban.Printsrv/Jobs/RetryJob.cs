using CorkbanPrintsrv.Infrastructure;

namespace CorkbanPrintsrv.Jobs;

public class RetryJob(ISqliteProvider sqliteProvider, IPrinterProvider printerProvider) : BackgroundService
{
    private const string JobId = "PrintQueueRetryJob_60Sec";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _ = Task.Run(async () => await ExecuteRetryJob(), stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    private async Task ExecuteRetryJob()
    {
        // find period to check
        var lastRun = await sqliteProvider.GetRetryJobLastRun(JobId);
        var thisRun = DateTime.UtcNow;

        // window is either:
        // - all time, if this job has never run
        // - last 7 days minus one second, if last run was within the last 7 days
        // - since last run minus one second, if last run was more than 7 days ago
        var periodStart = DateTime.UtcNow.Subtract(TimeSpan.FromDays(365 * 100));
        if (lastRun != null)
        {
            periodStart = thisRun.Subtract(TimeSpan.FromDays(7));
            if (lastRun.Value < periodStart) periodStart = lastRun.Value;
        }

        // shift window, just in case we somehow race-condition ourselves into a dupe print
        var shiftedStart = periodStart.Subtract(TimeSpan.FromSeconds(1));
        var shiftedEnd = thisRun.Subtract(TimeSpan.FromSeconds(1));

        // find all incomplete jobs in period
        var printJobsToRetry = await sqliteProvider.QueryIncompleteItemsBetween(shiftedStart, shiftedEnd);

        // send each print job to printer
        foreach (var job in printJobsToRetry)
        {
            if (job.Data is null)
            {
                await sqliteProvider.UpdateItemMessage(job.Id, "Attempted retry but data is null");
                continue;
            }

            try
            {
                await printerProvider.PrintAsync(job.Data);
                await sqliteProvider.UpdateItemMessage(job.Id, "Successful on retry");
                await sqliteProvider.CompleteItem(job.Id);
            }
            catch (Exception ex)
            {
                await sqliteProvider.UpdateItemMessage(job.Id, $"Failed on retry: {ex.Message}");
            }
        }

        await sqliteProvider.UpdateRetryJobLastRun(JobId, thisRun);
    }
}