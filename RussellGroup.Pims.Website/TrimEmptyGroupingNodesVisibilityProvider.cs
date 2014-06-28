using MvcSiteMapProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website
{
    public class TrimEmptyGroupingNodesVisibilityProvider : SiteMapNodeVisibilityProviderBase
    {
        public override bool IsVisible(ISiteMapNode node, IDictionary<string, object> sourceMetadata)
        {
            if (!node.HasChildNodes && !node.Clickable)
            {
                return false;
            }

            return true;
        }
    }
}