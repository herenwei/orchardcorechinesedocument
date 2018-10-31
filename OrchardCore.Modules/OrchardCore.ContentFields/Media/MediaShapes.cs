using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.ContentFields.Media
{
    public class MediaShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("HtmlField_Edit")
                .OnDisplaying(displaying =>
                {
                    IShape editor = displaying.Shape;

                    if (editor.Metadata.Type == "HtmlField_Edit__Wysiwyg")
                    {
                        editor.Metadata.Wrappers.Add("Media_Wrapper__HtmlField");
                    }
                });
        }
    }
}
