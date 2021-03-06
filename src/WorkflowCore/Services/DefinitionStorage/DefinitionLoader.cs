﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Primitives;
using WorkflowCore.Models.DefinitionStorage.v1;
using WorkflowCore.Exceptions;

namespace WorkflowCore.Services.DefinitionStorage
{
    public class DefinitionLoader : IDefinitionLoader
    {
        private readonly IWorkflowRegistry _registry;

        public DefinitionLoader(IWorkflowRegistry registry)
        {
            _registry = registry;
        }

        public WorkflowDefinition LoadDefinition(string json)
        {
            var source = JsonConvert.DeserializeObject<DefinitionSourceV1>(json);
            var def = Convert(source);
            _registry.RegisterWorkflow(def);
            return def;
        }

        private WorkflowDefinition Convert(DefinitionSourceV1 source)
        {
            var dataType = typeof(object);
            if (!string.IsNullOrEmpty(source.DataType))
                dataType = FindType(source.DataType);

            var result = new WorkflowDefinition
            {
                Id = source.Id,
                Version = source.Version,
                Steps = ConvertSteps(source.Steps, dataType),
                DefaultErrorBehavior = source.DefaultErrorBehavior,
                DefaultErrorRetryInterval = source.DefaultErrorRetryInterval,
                Description = source.Description,
                DataType = dataType
            };

            return result;
        }


        private List<WorkflowStep> ConvertSteps(ICollection<StepSourceV1> source, Type dataType)
        {
            var result = new List<WorkflowStep>();
            int i = 0;
            var stack = new Stack<StepSourceV1>(source.Reverse());
            var parents = new List<StepSourceV1>();
            var compensatables = new List<StepSourceV1>();

            while (stack.Count > 0)
            {
                var nextStep = stack.Pop();

                var stepType = FindType(nextStep.StepType);
                var containerType = typeof(WorkflowStep<>).MakeGenericType(stepType);
                var targetStep = (containerType.GetConstructor(new Type[] { }).Invoke(null) as WorkflowStep);

                if (!string.IsNullOrEmpty(nextStep.CancelCondition))
                {
                    containerType = typeof(CancellableStep<,>).MakeGenericType(stepType, dataType);
                    var cancelExprType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(dataType, typeof(bool)));
                    var dataParameter = Expression.Parameter(dataType, "data");
                    var cancelExpr = DynamicExpressionParser.ParseLambda(new[] { dataParameter }, typeof(bool), nextStep.CancelCondition);
                    targetStep = (containerType.GetConstructor(new Type[] { cancelExprType }).Invoke(new[] { cancelExpr }) as WorkflowStep);
                }

                if (nextStep.Saga)  //TODO: cancellable saga???
                {
                    containerType = typeof(SagaContainer<>).MakeGenericType(stepType);
                    targetStep = (containerType.GetConstructor(new Type[] { }).Invoke(null) as WorkflowStep);
                }

                targetStep.Id = i;
                targetStep.Name = nextStep.Name;
                targetStep.ErrorBehavior = nextStep.ErrorBehavior;
                targetStep.RetryInterval = nextStep.RetryInterval;
                targetStep.Tag = $"{nextStep.Id}";

                AttachInputs(nextStep, dataType, stepType, targetStep);
                AttachOutputs(nextStep, dataType, stepType, targetStep);

                if (nextStep.Do != null)
                {
                    foreach (var child in nextStep.Do.Reverse<StepSourceV1>())
                        stack.Push(child);

                    if (nextStep.Do.Count > 0)
                        parents.Add(nextStep);
                }

                if (nextStep.CompensateWith != null)
                {
                    foreach (var compChild in nextStep.CompensateWith.Reverse<StepSourceV1>())
                        stack.Push(compChild);

                    if (nextStep.CompensateWith.Count > 0)
                        compensatables.Add(nextStep);
                }

                if (!string.IsNullOrEmpty(nextStep.NextStepId))
                    targetStep.Outcomes.Add(new StepOutcome() { Tag = $"{nextStep.NextStepId}" });

                result.Add(targetStep);

                i++;
            }

            foreach (var step in result)
            {
                if (result.Any(x => string.Compare(x.Tag, step.Tag, true) == 0 && x.Id != step.Id))
                    throw new WorkflowDefinitionLoadException($"Duplicate step Id {step.Tag}");

                foreach (var outcome in step.Outcomes)
                {
                    if (result.All(x => string.Compare(x.Tag, outcome.Tag, true) != 0))
                    {
                        //throw new WorkflowDefinitionLoadException($"Cannot find step id {outcome.Tag}");
                        outcome.NextStep = -1;
                    }
                    else
                    {
                        outcome.NextStep = result.Single(x => string.Compare(x.Tag, outcome.Tag, true) == 0).Id;
                    }
                }
            }

            foreach (var parent in parents)
            {
                var target = result.Single(x => string.Compare(x.Tag, parent.Id, true) == 0);

                var childTags = parent.Do.Select(x => x.Id).ToArray();
                target.Children.AddRange(result
                    .Where(x => childTags.Contains(x.Tag))
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .Take(1)
                    .ToArray());
            }

            foreach (var item in compensatables)
            {
                var target = result.Single(x => string.Compare(x.Tag, item.Id, true) == 0);

                var tag = item.CompensateWith.Select(x => x.Id).FirstOrDefault();
                if (tag != null)
                {
                    var compStep = result.FirstOrDefault(x => string.Compare(x.Tag, tag, true) == 0);
                    if (compStep != null)
                    {
                        target.CompensationStepId = compStep.Id;
                    }
                }
            }

            return result;
        }

        private void AttachInputs(StepSourceV1 source, Type dataType, Type stepType, WorkflowStep step)
        {
            foreach (var input in source.Inputs)
            {
                var dataParameter = Expression.Parameter(dataType, "data");
                var contextParameter = Expression.Parameter(typeof(IStepExecutionContext), "context");
                var sourceExpr = DynamicExpressionParser.ParseLambda(new[] { dataParameter, contextParameter }, typeof(object), input.Value);
                var targetExpr = Expression.Property(Expression.Parameter(stepType), input.Key);

                step.Inputs.Add(new DataMapping()
                {
                    Source = sourceExpr,
                    Target = Expression.Lambda(targetExpr)
                });
            }
        }

        private void AttachOutputs(StepSourceV1 source, Type dataType, Type stepType, WorkflowStep step)
        {
            foreach (var output in source.Outputs)
            {
                var stepParameter = Expression.Parameter(stepType, "step");
                var sourceExpr = DynamicExpressionParser.ParseLambda(new[] { stepParameter }, typeof(object), output.Value);
                var targetExpr = Expression.Property(Expression.Parameter(dataType), output.Key);

                step.Outputs.Add(new DataMapping()
                {
                    Source = sourceExpr,
                    Target = Expression.Lambda(targetExpr)
                });
            }
        }

        private Type FindType(string name)
        {
            return Type.GetType(name, true, true);
        }

    }
}
