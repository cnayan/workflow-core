using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Sample02.Steps;

namespace WorkflowCore.Sample02
{
    public class SimpleDecisionWorkflow : IWorkflow
    {
        public string Id => "Simple Decision Workflow";

        public int Version => 1;

        public void Build(IWorkflowBuilder<object> builder)
        {
            builder
                .StartWith<HelloWorld>()
                .Then<RandomOutput>(randomOutput =>
                {
                    randomOutput
                        .When(_ => 0).Do(then =>
                            then
                                .StartWith(ctx => ctx.Step.Name = "Print custom message")
                                .Then<CustomMessage>()
                                    .Input(input => input.Message, _ => "Looping back....")
                                .Then(randomOutput)  //loop back to randomOutput
                        );

                    randomOutput
                        .When(_ => 1)
                        .Do(then => then.StartWith<GoodbyeWorld>());
                });
        }
    }
}
