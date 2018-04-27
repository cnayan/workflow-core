using System;
using System.Collections.Generic;
using WorkflowCore.Interface;

namespace WorkflowCore.Models
{
    public abstract class WorkflowStep
    {
        public abstract Type BodyType { get; }

        public virtual List<int> Children { get; set; } = new List<int>();
        public virtual int? CompensationStepId { get; set; }
        public virtual WorkflowErrorHandling? ErrorBehavior { get; set; }
        public virtual int Id { get; set; }
        public virtual List<DataMapping> Inputs { get; set; } = new List<DataMapping>();
        public virtual string Name { get; set; }
        public virtual List<StepOutcome> Outcomes { get; set; } = new List<StepOutcome>();
        public virtual List<DataMapping> Outputs { get; set; } = new List<DataMapping>();
        public virtual bool ResumeChildrenAfterCompensation => true;
        public virtual TimeSpan? RetryInterval { get; set; }
        public virtual bool RevertChildrenAfterCompensation => false;
        public virtual string Tag { get; set; }

        public virtual void AfterExecute(WorkflowExecutorResult executorResult, IStepExecutionContext context, ExecutionResult stepResult, ExecutionPointer executionPointer)
        {
        }

        /// <summary>
        /// Called after every workflow execution round,
        /// every exectuon pointer with no end time, even if this step was not executed in this round
        /// </summary>
        /// <param name="executorResult"></param>
        /// <param name="defintion"></param>
        /// <param name="workflow"></param>
        /// <param name="executionPointer"></param>
        public virtual void AfterWorkflowIteration(WorkflowExecutorResult executorResult, WorkflowDefinition defintion, WorkflowInstance workflow, ExecutionPointer executionPointer)
        {
        }

        public virtual ExecutionPipelineDirective BeforeExecute(WorkflowExecutorResult executorResult, IStepExecutionContext context, ExecutionPointer executionPointer, IStepBody body)
        {
            return ExecutionPipelineDirective.Next;
        }

        public virtual IStepBody ConstructBody(IServiceProvider serviceProvider)
        {
            IStepBody body = (serviceProvider.GetService(BodyType) as IStepBody);
            if (body == null)
            {
                object[] parameters = null;
                var stepCtor = BodyType.GetConstructor(new Type[] { typeof(IServiceProvider) });
                if (stepCtor != null)
                {
                    parameters = new object[] { serviceProvider };
                }
                else
                {
                    stepCtor = BodyType.GetConstructor(new Type[] { });
                }

                if (stepCtor != null)
                {
                    body = (stepCtor.Invoke(parameters) as IStepBody);
                }
            }

            return body;
        }

        public virtual ExecutionPipelineDirective InitForExecution(WorkflowExecutorResult executorResult, WorkflowDefinition defintion, WorkflowInstance workflow, ExecutionPointer executionPointer)
        {
            return ExecutionPipelineDirective.Next;
        }

        public virtual void PrimeForRetry(ExecutionPointer pointer)
        {
        }
    }

    public class WorkflowStep<TStepBody> : WorkflowStep where TStepBody : IStepBody
    {
        public override Type BodyType => typeof(TStepBody);
    }
}
