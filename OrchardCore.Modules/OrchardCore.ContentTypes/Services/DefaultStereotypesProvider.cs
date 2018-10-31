﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentTypes.Services
{
    public class DefaultStereotypesProvider : IStereotypesProvider
    {
        private readonly Lazy<IContentDefinitionService> _contentDefinitionService;
        public DefaultStereotypesProvider(Lazy<IContentDefinitionService> contentDefinitionService)
        {
            _contentDefinitionService = contentDefinitionService;
        }

        public IEnumerable<StereotypeDescription> GetStereotypes()
        {
            // Harvest all available stereotypes by finding out about the stereotype of all content types
            var stereotypes = _contentDefinitionService.Value.GetTypes().Where(x => x.Settings["Stereotype"] != null).Select(x => x.Settings["Stereotype"].ToString()).Distinct();
            return stereotypes.Select(x => new StereotypeDescription { DisplayName = x, Stereotype = x });
        }
    }
}