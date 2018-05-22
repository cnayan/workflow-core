using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ScratchPad
{
    public class PrintMessage : StepBody
    {
        public string Message { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine(Message);
            return ExecutionResult.Next();
        }
    }
}

