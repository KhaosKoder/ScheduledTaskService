public class SQLTask
{
    public string ConnectionString { get; set; } = string.Empty;
    public string SQLCommandText { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
}
