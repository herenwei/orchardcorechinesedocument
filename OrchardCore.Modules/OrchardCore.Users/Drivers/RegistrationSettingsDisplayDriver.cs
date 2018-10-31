using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    [Feature("OrchardCore.Users.Registration")]
    public class RegistrationSettingsDisplayDriver : SectionDisplayDriver<ISite, RegistrationSettings>
    {
        public const string GroupId = "RegistrationSettings";

        public override IDisplayResult Edit(RegistrationSettings section)
        {
            return Initialize<RegistrationSettings>("RegistrationSettings_Edit", model => {
                model.UsersCanRegister = section.UsersCanRegister;
                model.UsersMustValidateEmail = section.UsersMustValidateEmail;
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(RegistrationSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                await context.Updater.TryUpdateModelAsync(section, Prefix);
            }
            return await EditAsync(section, context);
        }
    }
}
