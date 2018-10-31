using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class ForLoopTask : TaskActivity
    {
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;

        public ForLoopTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<ForLoopTask> localizer)
        {
            _scriptEvaluator = scriptEvaluator;
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(ForLoopTask);
        public override LocalizedString Category => T["Control Flow"];

        /// <summary>
        /// An expression evaluating to the start value.
        /// </summary>
        public WorkflowExpression<double> From
        {
            get => GetProperty(() => new WorkflowExpression<double>("0"));
            set => SetProperty(value);
        }

        /// <summary>
        /// An expression evaluating to the end value.
        /// </summary>
        public WorkflowExpression<double> To
        {
            get => GetProperty(() => new WorkflowExpression<double>("10"));
            set => SetProperty(value);
        }

        /// <summary>
        /// The property name to store the current iteration number in.
        /// </summary>
        public string LoopVariableName
        {
            get => GetProperty(() => "x");
            set => SetProperty(value);
        }

        /// <summary>
        /// The current index of the iteration.
        /// </summary>
        public double Index
        {
            get => GetProperty(() => 0);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Iterate"], T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            if (!double.TryParse(From.Expression, out var from))
            {
                from = await _scriptEvaluator.EvaluateAsync(From, workflowContext);
            }

            if (!double.TryParse(To.Expression, out var to))
            {
                to = await _scriptEvaluator.EvaluateAsync(To, workflowContext);
            }

            if(Index < from)
            {
                Index = from;
            }

            if (Index < to)
            {
                workflowContext.LastResult = Index;
                workflowContext.Properties[LoopVariableName] = Index;
                Index++;
                return Outcomes("Iterate");
            }
            else
            {
                Index = from;
                return Outcomes("Done");
            }
        }
    }
}