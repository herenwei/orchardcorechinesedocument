using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers
{
    public class FormElementPartDisplay : ContentPartDisplayDriver<FormElementPart>
    {
        public override IDisplayResult Edit(FormElementPart part, BuildPartEditorContext context)
        {
            return Initialize<FormElementPartEditViewModel>("FormElementPart_Fields_Edit", m =>
            {
                m.Id = part.Id;
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(FormElementPart part, IUpdateModel updater)
        {
            var viewModel = new FormElementPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.Id = viewModel.Id?.Trim();
            }

            return Edit(part);
        }
    }
}
