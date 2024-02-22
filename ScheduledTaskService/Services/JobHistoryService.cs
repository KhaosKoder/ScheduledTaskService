public class JobHistoryService
{
    private readonly List<JobHistory> _jobHistories = new();

    public void AddJobHistory(JobHistory jobHistory)
    {
        _jobHistories.Add(jobHistory);
    }

    public IEnumerable<JobHistory> GetRecentJobHistories()
    {
        return _jobHistories.Where(j => j.StartTime >= DateTime.Now.AddHours(-48)).ToList();
    }
    public DateTime? GetLastRunTime(string jobName)
    {
        var lastRun = _jobHistories
            .Where(j => j.JobName == jobName && j.Status == "Success")
            .OrderByDescending(j => j.StartTime)
            .FirstOrDefault();
        return lastRun?.StartTime;
    }

}
