using Dapper;
using Microsoft.Data.SqlClient;
using Quartz;

[DisallowConcurrentExecution]
public class SQLTaskJob : IJob
{
    private readonly ILogger<SQLTask> _logger;
    private readonly JobHistoryService _jobHistoryService;

    public SQLTaskJob(ILogger<SQLTask> logger, JobHistoryService jobHistoryService)
    {
        this._logger = logger;
        _jobHistoryService = jobHistoryService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.MergedJobDataMap;
        string connectionString = dataMap.GetString("ConnectionString");
        string sqlCommandText = dataMap.GetString("SQLCommandText");

        try
        {
            //using (var connection = new SqlConnection(connectionString))
            //{
            //    await connection.ExecuteAsync(sqlCommandText);
            //}
            _logger.LogInformation("Execute SQLTask");
            _jobHistoryService.AddJobHistory(new JobHistory { JobName = context.JobDetail.Key.Name, StartTime = DateTime.Now, Status = "Success" });
        }
        catch (Exception ex)
        {
            _jobHistoryService.AddJobHistory(new JobHistory { JobName = context.JobDetail.Key.Name, StartTime = DateTime.Now, Status = $"Failed: {ex.Message}" });
            throw;
        }
    }
}
