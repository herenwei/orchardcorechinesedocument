using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class SetPropertyTask : TaskActivity
    {
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;

        public SetPropertyTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<SetPropertyTask> localizer)
        {
            _scriptEvaluator = scriptEvaluator;
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(SetPropertyTask);
        public override LocalizedString Category => T["Primitives"];

        public string PropertyName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public WorkflowExpression<object> Value
        {
            get => GetProperty(() => new WorkflowExpression<object>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var value = await _scriptEvaluator.EvaluateAsync(Value, workflowContext);
            workflowContext.Properties[PropertyName] = value;

            return Outcomes("Done");
        }
    }
}