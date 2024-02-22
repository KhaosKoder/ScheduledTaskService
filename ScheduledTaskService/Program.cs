using Microsoft.OpenApi.Models;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using System.Diagnostics;
using System.Data.SqlClient;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Scheduled Tasks API", Version = "v1" });
});

// Configure Quartz
var scheduledTaskSettings = new ScheduledTaskSettings();
builder.Configuration.GetSection("ScheduledTasks").Bind(scheduledTaskSettings);

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Dynamically add jobs and triggers for SQL tasks
    foreach (var sqlTask in scheduledTaskSettings.SQLTasks)
    {
        var jobKey = new JobKey(sqlTask.JobName);
        q.AddJob<SQLTaskJob>(opts => opts.WithIdentity(jobKey).UsingJobData(new JobDataMap
        {
            {"ConnectionString", sqlTask.ConnectionString},
            {"SQLCommandText", sqlTask.SQLCommandText}
        }));

        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{sqlTask.JobName}-trigger")
            .WithCronSchedule(sqlTask.CronExpression));
    }

    // Dynamically add jobs and triggers for script tasks
    foreach (var scriptTask in scheduledTaskSettings.ScriptTasks)
    {
        var jobKey = new JobKey(scriptTask.JobName);
        q.AddJob<ScriptTaskJob>(opts => opts.WithIdentity(jobKey).UsingJobData(new JobDataMap
        {
            {"ScriptFileName", scriptTask.ScriptFileName}
        }));

        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{scriptTask.JobName}-trigger")
            .WithCronSchedule(scriptTask.CronExpression));
    }
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


// Custom services
builder.Services.AddSingleton<JobHistoryService>();
builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();   

app.Run();
