using System;
using Microsoft.SharePoint;

namespace Bugfree.SharePoint.Common.Source.Extensions
{
    public static class SPViewCollectionExtensions
    {
        // a view doesn't have an internal name, only a Guid that uniquely identifies it. Hence lookup by display name
        public static SPView TryGetViewByDisplayName(this SPViewCollection views, string displayName)
        {
            SPView view = null;
            try
            {
                view = views[displayName];
            }
            catch (ArgumentException)
            {
                // intentionally left black
            }
            return view;
        }

        public static void DeleteViewIfExistsByDisplayName(this SPViewCollection views, string displayName)
        {
            var view = views.TryGetViewByDisplayName(displayName);
            if (view != null)
            {
                var list = view.ParentList;
                list.Views.Delete(view.ID);
                list.Update();
            }
        }
    }
}
