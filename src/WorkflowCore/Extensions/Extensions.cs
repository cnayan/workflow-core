using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Creates a new workflow step and adds it to the workflow builder
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TStep"></typeparam>
        /// <param name="workflowBuilder"></param>
        public static WorkflowStep<TStep> AddStep<TData, TStep>(this IWorkflowBuilder<TData> workflowBuilder) where TStep : IStepBody
        {
            var step = new WorkflowStep<TStep>();
            step.Name = step.Name ?? typeof(TStep).Name;
            workflowBuilder.AddStep(step);
            return step;
        }

        public static object CreateInstance(this Type t, IServiceProvider serviceProvider)
        {
            object[] parameters = null;
            var stepCtor = t.GetConstructor(new Type[] { typeof(IServiceProvider) });
            if (stepCtor != null)
            {
                parameters = new object[] { serviceProvider };
            }
            else
            {
                stepCtor = t.GetConstructor(new Type[] { });
            }

            if (stepCtor != null)
            {
                return stepCtor.Invoke(parameters) as IStepBody;
            }

            return null;
        }
    }
}