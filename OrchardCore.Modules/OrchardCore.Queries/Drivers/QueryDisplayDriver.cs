using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Queries.ViewModels;

namespace OrchardCore.Queries.Drivers
{
    public class QueryDisplayDriver : DisplayDriver<Query>
    {
        private readonly IQueryManager _queryManager;
        private readonly IStringLocalizer<QueryDisplayDriver> S;

        public QueryDisplayDriver(IQueryManager queryManager, IStringLocalizer<QueryDisplayDriver> stringLocalizer)
        {
            _queryManager = queryManager;
            S = stringLocalizer;
        }

        public override IDisplayResult Display(Query query, IUpdateModel updater)
        {
            return Combine(
                Dynamic("Query_Fields_SummaryAdmin", model =>
                {
                    model.Name = query.Name;
                    model.Source = query.Source;
                    model.Query = query;
                }).Location("Content:1"),
                Dynamic("Query_Buttons_SummaryAdmin", model =>
                {
                    model.Name = query.Name;
                    model.Source = query.Source;
                    model.Query = query;
                }).Location("Actions:5")
            );
        }

        public override IDisplayResult Edit(Query query, IUpdateModel updater)
        {
            return Combine(
                Initialize<EditQueryViewModel>("Query_Fields_Edit", model =>
                {
                    model.Name = query.Name;
                    model.Source = query.Source;
                    model.Query = query;
                }).Location("Content:1"),
                Initialize<EditQueryViewModel>("Query_Fields_Buttons", model =>
                {
                    model.Name = query.Name;
                    model.Source = query.Source;
                    model.Query = query;
                }).Location("Actions:5")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(Query model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, m => m.Name, m => m.Source);

            if (String.IsNullOrEmpty(model.Name))
            {
                updater.ModelState.AddModelError(nameof(model.Name), S["Name is required"]);
            }

            var safeName = model.Name.ToSafeName();
            if (String.IsNullOrEmpty(safeName) || model.Name != safeName)
            {
                updater.ModelState.AddModelError(nameof(model.Name), S["Name contains illegal characters"]);
            }
            else
            {
                var existing = await _queryManager.GetQueryAsync(safeName);

                if (existing != null && existing != model)
                {
                    updater.ModelState.AddModelError(nameof(model.Name), S["A query with the same name already exists"]);
                }
            }

            return await EditAsync(model, updater);
        }
    }
}
