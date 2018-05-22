using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ScratchPad
{
    public class GenerateMessage : StepBody
    {
        public string Message { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Message = "Generated message";
            return ExecutionResult.Next();
        }
    }
}

