using Quartz;
using System.Diagnostics;

[DisallowConcurrentExecution]
public class ScriptTaskJob : IJob
{
    private readonly ILogger<ScriptTask> _logger;
    private readonly JobHistoryService _jobHistoryService;

    public ScriptTaskJob(ILogger<ScriptTask> logger, JobHistoryService jobHistoryService)
    {
        this._logger = logger;
        _jobHistoryService = jobHistoryService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.MergedJobDataMap;
        string scriptFileName = dataMap.GetString("ScriptFileName");

        try
        {
            _logger.LogInformation("Execute Script task"); 
            var processStartInfo = new ProcessStartInfo
            {
                FileName = scriptFileName.EndsWith(".ps1") ? "powershell.exe" : "cmd.exe",
                Arguments = scriptFileName.EndsWith(".ps1") ? $"-NoProfile -ExecutionPolicy Unrestricted -File \"{scriptFileName}\"" : $"/c \"{scriptFileName}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(processStartInfo))
            {
                await process.WaitForExitAsync();
            }
            _jobHistoryService.AddJobHistory(new JobHistory { JobName = context.JobDetail.Key.Name, StartTime = DateTime.Now, Status = "Success" });
        }
        catch (Exception ex)
        {
            _jobHistoryService.AddJobHistory(new JobHistory { JobName = context.JobDetail.Key.Name, StartTime = DateTime.Now, Status = $"Failed: {ex.Message}" });
            throw;
        }
    }
}
