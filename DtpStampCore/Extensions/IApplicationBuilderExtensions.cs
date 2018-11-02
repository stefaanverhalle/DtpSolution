﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DtpStampCore.Interfaces;
using DtpStampCore.Workflows;
using DtpCore.Services;

namespace DtpStampCore.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static void DtpStamp(this IApplicationBuilder app)
        {
            // Ensure that a Timestamp workflow is running.
            using (var scope = app.ApplicationServices.CreateScope())
            {
                //var timestampWorkflowService = scope.ServiceProvider.GetRequiredService<ITimestampWorkflowService>();
                //timestampWorkflowService.EnsureTimestampScheduleWorkflow();
                //timestampWorkflowService.CreateAndExecute(); // Make sure that there is a Timestamp engine workflow
                var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                workflowService.EnsureWorkflow<CreateProofWorkflow>();
                workflowService.EnsureWorkflow<UpdateProofWorkflow>();

            }
        }
    }
}
