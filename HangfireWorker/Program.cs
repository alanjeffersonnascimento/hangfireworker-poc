using Cronos;
using Hangfire;
using Hangfire.Common;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseHangfireDashboard();

//BackgroundJobClient backgroundJobs = new BackgroundJobClient();
//backgroundJobs.Enqueue(() => Console.WriteLine("Hello world from Hangfire!"));

var manager = new RecurringJobManager();
manager.AddOrUpdate("thursday-job", Job.FromExpression(() => Console.Write("Execution from thursday-job")), "20 14 * * 4");

//RecurringJob.RemoveIfExists("some-id");
//RecurringJob.Trigger("some-id");

CronExpression expression = CronExpression.Parse("* * * * *");
DateTime? nextUtc = expression.GetNextOccurrence(DateTime.UtcNow);

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHangfireDashboard();
});

app.MapRazorPages();

app.Run();
