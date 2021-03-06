﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using WorkflowCore.Interface;

namespace WorkflowCore.Sample06
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IServiceProvider serviceProvider = ConfigureServices();

            //start the workflow host
            var host = serviceProvider.GetService<IWorkflowHost>();
            host.RegisterWorkflow<MultipleOutcomeWorkflow>();
            host.Start();

            host.StartWorkflow("MultipleOutcomeWorkflow", 1, null);

            Console.ReadLine();
            host.Stop();
        }

        private static IServiceProvider ConfigureServices()
        {
            //setup dependency injection
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddWorkflow();
            //services.AddWorkflow(x => x.UseSqlServer(@"Server=.;Database=Test3;Trusted_Connection=True;", true, true));            

            var serviceProvider = services.BuildServiceProvider();

            //config logging
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddDebug();
            return serviceProvider;
        }
    }
}
