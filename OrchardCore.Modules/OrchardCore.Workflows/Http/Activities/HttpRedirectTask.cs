using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Activities
{
    public class HttpRedirectTask : TaskActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public HttpRedirectTask(
            IStringLocalizer<HttpRedirectTask> localizer,
            IHttpContextAccessor httpContextAccessor,
            IWorkflowExpressionEvaluator expressionEvaluator
        )
        {
            T = localizer;
            _httpContextAccessor = httpContextAccessor;
            _expressionEvaluator = expressionEvaluator;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(HttpRedirectTask);
        public override LocalizedString Category => T["HTTP"];

        public WorkflowExpression<string> Location
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public bool Permanent
        {
            get => GetProperty(() => false);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var location = await _expressionEvaluator.EvaluateAsync(Location, workflowContext);

            _httpContextAccessor.HttpContext.Response.Redirect(location, Permanent);
            _httpContextAccessor.HttpContext.Items[WorkflowHttpResult.Instance] = WorkflowHttpResult.Instance;

            return Outcomes("Done");
        }
    }
}