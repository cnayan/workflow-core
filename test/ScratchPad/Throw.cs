using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ScratchPad
{
    public class Throw : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine("throwing...");
            throw new Exception("up");
        }
    }
}

