﻿// Source:
// https://blog.maartenballiauw.be/post/2017/08/01/building-a-scheduled-cache-updater-in-aspnet-core-2.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Strategy.Cron;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DtpCore.Services
{
    public class SchedulerHostedService : BackgroundService
    {
        private CancellationTokenSource DelayTokenSource = null;
        //private static CancellationToken DelayToken = new CancellationToken();

        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;


        public void RunNow()
        {
            //DelayTokenSource.Cancel();  // Cancel current token.
            DelayTokenSource.CancelAfter(1);
            //DelayToken = DelayTokenSource.Token; // Create new token.
        }

        public SchedulerHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<SchedulerHostedService> logger, IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _configuration = configuration;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            DelayTokenSource = new CancellationTokenSource();
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Scheduler Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(cancellationToken);

                // Hack
                var count = 0; // WorkflowInterval is in seconds
                while (!DelayTokenSource.Token.IsCancellationRequested && count++ < _configuration.WorkflowInterval() * 1000)
                {
                    await Task.Delay(1); // Cannot get Cancellation token to work with Task.Deplay
                }

                if (DelayTokenSource.IsCancellationRequested)
                    DelayTokenSource = new CancellationTokenSource();
            }

            _logger.LogDebug($"Scheduler Service is stopping.");
        }

        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            var taskFactory = new TaskFactory(TaskScheduler.Current);

            // Get all workflows group by type.
            var workflowContainers = GetWorkflowContainers();

            // Run containers
            foreach (var entry in workflowContainers)
            {
                await taskFactory.StartNew(
                    RunContainers(entry.Value, cancellationToken),
                    cancellationToken);
            }
        }

        private Dictionary<string, IList<WorkflowContainer>> GetWorkflowContainers()
        {
            var workflowContainers = new Dictionary<string, IList<WorkflowContainer>>();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var list = workflowService.GetRunningWorkflows();

                foreach (var item in list)
                {
                    if (!workflowContainers.ContainsKey(item.Type))
                        workflowContainers[item.Type] = new List<WorkflowContainer>();

                    workflowContainers[item.Type].Add(item);
                }
            }

            return workflowContainers;
        }

        private Func<Task> RunContainers(IList<WorkflowContainer> containers, CancellationToken cancellationToken)
        {
            return async () =>
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        // Run all the same workflow type in a sequential order
                        foreach (var container in containers)
                        {
                            var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                            var workflowInstance = workflowService.Create(container);

                            var t = workflowInstance.GetType();
                            var method = t.GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public);
                            var arguments = method.GetParameters()
                                            .Select(a => a.ParameterType == typeof(CancellationToken) ? cancellationToken : scope.ServiceProvider.GetService(a.ParameterType))
                                            .ToArray();


                            container.State = WorkflowStatusType.Running.ToString();
                            workflowService.Save(workflowInstance);

                            //invoke.
                            if (typeof(Task).Equals(method.ReturnType))
                            {
                                await (Task)method.Invoke(workflowInstance, arguments);
                            }
                            else
                            {
                                method.Invoke(workflowInstance, arguments);
                            }


                            if (container.State == WorkflowStatusType.Running.ToString())
                            {
                                container.State = WorkflowStatusType.Waiting.ToString();
                                workflowService.Save(workflowInstance);
                            }
                        }

                    }

                }
                catch (Exception ex)
                {
                    var args = new UnobservedTaskExceptionEventArgs(
                        ex as AggregateException ?? new AggregateException(ex));

                    UnobservedTaskException?.Invoke(this, args);

                    //_logger.LogError(ex, ex.Message);


                    if (!args.Observed)
                    {
                        throw;
                    }
                }
            };
        }

        private class SchedulerTaskWrapper
        {
            public CrontabSchedule Schedule { get; set; }
            public IScheduledTask Task { get; set; }

            public DateTime LastRunTime { get; set; }
            public DateTime NextRunTime { get; set; }

            public void Increment()
            {
                LastRunTime = NextRunTime;
                NextRunTime = Schedule.GetNextOccurrence(NextRunTime);
            }

            public bool ShouldRun(DateTime currentTime)
            {
                return NextRunTime < currentTime && LastRunTime != NextRunTime;
            }
        }
    }
}