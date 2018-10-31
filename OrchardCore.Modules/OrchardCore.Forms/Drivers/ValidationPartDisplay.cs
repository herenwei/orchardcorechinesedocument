using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers
{
    public class ValidationPartDisplay : ContentPartDisplayDriver<ValidationPart>
    {
        public override IDisplayResult Display(ValidationPart part)
        {
            return View("ValidationPart", part).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(ValidationPart part, BuildPartEditorContext context)
        {
            return Initialize<ValidationPartEditViewModel>("ValidationPart_Fields_Edit", m =>
            {
                m.For = part.For;
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(ValidationPart part, IUpdateModel updater)
        {
            var viewModel = new ValidationPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.For = viewModel.For?.Trim();
            }

            return Edit(part);
        }
    }
}
