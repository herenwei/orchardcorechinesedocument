using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    /// <summary>
    /// A replacement for when a referenced activity could not be found. This is a system-level activity that is not registered with WorkflowOptions.
    /// </summary>
    public class MissingActivity : Activity
    {
        private readonly ILogger<MissingActivity> _logger;

        public MissingActivity(IStringLocalizer<MissingActivity> localizer, ILogger<MissingActivity> logger, ActivityRecord missingActivityRecord)
        {
            T = localizer;
            _logger = logger;
            MissingActivityRecord = missingActivityRecord;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(MissingActivity);
        public override LocalizedString Category => T["Exceptions"];

        public ActivityRecord MissingActivityRecord { get; }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            yield break;
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            _logger.LogWarning("Activity '{ActivityName}' is no longer available. This can happen if the feature providing the activity is no longer enabled. Either enable the feature, or remove this activity from workflow definition with ID {WorkflowTypeId}", MissingActivityRecord.Name, workflowContext.WorkflowType.Id);
            return Noop();
        }
    }
}