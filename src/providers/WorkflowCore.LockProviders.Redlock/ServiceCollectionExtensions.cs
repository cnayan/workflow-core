﻿using WorkflowCore.Models;
using WorkflowCore.LockProviders.Redlock.Services;
using System.Net;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static WorkflowOptions UseRedlock(this WorkflowOptions options, params DnsEndPoint[] endpoints)
        {
            options.UseDistributedLockManager(sp => new RedlockProvider(endpoints));
            return options;
        }
    }
}
