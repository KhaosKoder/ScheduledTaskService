using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl.Matchers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("[controller]")]
public class SchedulerController : ControllerBase
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly JobHistoryService _jobHistoryService;

    public SchedulerController(ISchedulerFactory schedulerFactory, JobHistoryService jobHistoryService)
    {
        _schedulerFactory = schedulerFactory;
        _jobHistoryService = jobHistoryService;
    }

    [HttpPost("start/{jobKey}")]
    public async Task<IActionResult> StartJob(string jobKey)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobDetail = await scheduler.GetJobDetail(new JobKey(jobKey));

        if (jobDetail != null)
        {
            await scheduler.TriggerJob(new JobKey(jobKey));
            return Ok($"Job {jobKey} started successfully.");
        }

        return NotFound($"Job {jobKey} not found.");
    }

    [HttpPost("stop/{jobKey}")]
    public async Task<IActionResult> StopJob(string jobKey)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        await scheduler.Interrupt(new JobKey(jobKey));
        return Ok($"Job {jobKey} stop request sent successfully.");
    }

    // Modified to include jobKey in executed tasks
    [HttpGet("history")]
    public IActionResult GetJobHistory()
    {
        var histories = _jobHistoryService.GetRecentJobHistories()
            .Select(history => new
            {
                history.JobKey, // Include JobKey explicitly if not already included
                history.JobName,
                history.StartTime,
                history.EndTime,
                history.Status
            });

        return Ok(histories);
    }

    // Existing method for scheduled jobs
    [HttpGet("scheduledJobs")]
    public async Task<IActionResult> GetScheduledJobs()
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        var jobsList = new List<object>();

        foreach (var jobKey in jobKeys)
        {
            var detail = await scheduler.GetJobDetail(jobKey);
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            var nextFireTime = triggers.Select(t => t.GetNextFireTimeUtc()?.LocalDateTime.ToString()).FirstOrDefault();
            var lastFireTime = triggers.Select(t => t.GetPreviousFireTimeUtc()?.LocalDateTime.ToString()).FirstOrDefault();

            jobsList.Add(new
            {
                JobKey = jobKey.Name,
                JobGroup = jobKey.Group,
                Description = detail.Description,
                LastRunTime = lastFireTime ?? "N/A",
                NextRunTime = nextFireTime ?? "N/A"
            });
        }

        return Ok(jobsList);
    }

    // New method to list all jobs with jobKey and name
    [HttpGet("allJobs")]
    public async Task<IActionResult> GetAllJobs()
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        var jobsList = new List<object>();

        foreach (var jobKey in jobKeys)
        {
            var detail = await scheduler.GetJobDetail(jobKey);
            jobsList.Add(new
            {
                JobKey = jobKey.Name,
                JobName = detail.Description // Assuming 'Description' contains the name. Adjust as needed.
            });
        }

        return Ok(jobsList);
    }
}
