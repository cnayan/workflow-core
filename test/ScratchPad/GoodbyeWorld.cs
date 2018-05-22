using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ScratchPad
{
    public class GoodbyeWorld : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine("Goodbye world");
            return ExecutionResult.Next();
        }
    }
}

