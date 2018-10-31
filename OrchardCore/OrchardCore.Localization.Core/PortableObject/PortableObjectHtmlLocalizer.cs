using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization.PortableObject
{
    public class PortableObjectHtmlLocalizer : HtmlLocalizer
    {
        private readonly IStringLocalizer _localizer;

        public string Context { get; private set; }

        public PortableObjectHtmlLocalizer(IStringLocalizer localizer) : base(localizer)
        {
            _localizer = localizer;
        }

        public override LocalizedHtmlString this[string name]
        {
            get
            {
                return ToHtmlString(_localizer[name]);
            }
        }

        public override LocalizedHtmlString this[string name, params object[] arguments]
        {
            get
            {
                // 'HtmlLocalizer' doesn't use '_localizer[name, arguments]' but '_localizer[name]' to get
                // an unformatted string because a formatting is done through 'LocalizedHtmlString.WriteTo()'.

                // See https://github.com/aspnet/Mvc/blob/master/src/Microsoft.AspNetCore.Mvc.Localization/LocalizedHtmlString.cs#L97

                // But with a plural localizer, arguments may be provided for plural localization. So, we
                // still use them to get a non formatted translation and extract all non plural arguments.

                // Otherwise an already formatted string containing curly braces will be wrongly reformatted.

                if (_localizer is IPluralStringLocalizer pluralLocalizer)
                {
                    // Get an unformatted string and all non plural arguments (1st one is the plural count).
                    var (translation, argumentsWithCount) = pluralLocalizer.GetTranslation(name, arguments);

                    // Formatting will use non plural arguments if any.
                    return ToHtmlString(translation, argumentsWithCount);
                }

                return ToHtmlString(_localizer[name], arguments);
            }
        }
    }
}
