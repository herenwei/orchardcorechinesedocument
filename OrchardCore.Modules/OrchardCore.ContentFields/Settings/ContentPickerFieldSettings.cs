namespace OrchardCore.ContentFields.Settings
{
    public class ContentPickerFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public bool Multiple { get; set; }
        public string[] DisplayedContentTypes { get; set; } = new string[0];
    }
}