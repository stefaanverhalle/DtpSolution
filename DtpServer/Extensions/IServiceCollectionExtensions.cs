﻿using DtpCore.Notifications;
using DtpServer.AspNetCore;
using DtpServer.Notifications;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;

namespace DtpServer.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpServer(this IServiceCollection services)
        {
            // https://github.com/Biarity/Sieve/issues/4#issuecomment-364629048
            services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();

        }
    }
}