using System;

namespace OrchardCore.Lucene.ViewModels
{
    public class LuceneIndexDeploymentStepViewModel
    {
        public bool IncludeAll { get; set; }
        public string[] IndexNames { get; set; }
        public string[] AllIndexNames { get; set; }
    }
}
