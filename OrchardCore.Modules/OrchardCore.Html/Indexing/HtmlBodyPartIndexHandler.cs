using System.Threading.Tasks;
using OrchardCore.Html.Model;
using OrchardCore.Indexing;

namespace OrchardCore.Html.Indexing
{
    public class HtmlBodyPartIndexHandler : ContentPartIndexHandler<HtmlBodyPart>
    {
        public override Task BuildIndexAsync(HtmlBodyPart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions() 
                | DocumentIndexOptions.Sanitize 
                | DocumentIndexOptions.Analyze
                ;

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, part.Html, options);
            }

            return Task.CompletedTask;
        }
    }
}
