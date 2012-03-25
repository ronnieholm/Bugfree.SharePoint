using System;
using System.Linq;
using Microsoft.SharePoint;

namespace Bugfree.SharePoint.Common.Source.Extensions
{
    public static class SPListCollectionExtensions
    {
        public static SPList TryGetListByInternalName(this SPListCollection lists, string internalName)
        {
            // RootFolder == internal listname
            return (from SPList l in lists
                    where l.RootFolder.Name.Equals(internalName, StringComparison.InvariantCulture)
                    select l).SingleOrDefault();
        }
    }
}