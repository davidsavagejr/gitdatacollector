using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Quartz;
using Quartz.Impl;

namespace DataCollectorService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("DataCollectorService is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            Scheduler = StdSchedulerFactory.GetDefaultScheduler();

            ConfigureJobs(Scheduler);

            Scheduler.Start();

            var result = base.OnStart();

            Trace.TraceInformation("DataCollectorService has been started");

            return result;
        }

        private static void ConfigureJobs(IScheduler scheduler)
        {
            var mainGitJob = JobBuilder.Create<CollectGitDataJob>()
                .WithIdentity("gitDataCore")
                .Build();

            var mainGitJobSchedule = TriggerBuilder.Create()
                .WithIdentity("gitDataCoreTrigger")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(24)
                    .RepeatForever())
                .Build();

            scheduler.ScheduleJob(mainGitJob, mainGitJobSchedule);
        }

        public IScheduler Scheduler { get; set; }

        public override void OnStop()
        {
            Trace.TraceInformation("DataCollectorService is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            Scheduler.Shutdown();

            base.OnStop();

            Trace.TraceInformation("DataCollectorService has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(10000);
            }
        }
    }
}
