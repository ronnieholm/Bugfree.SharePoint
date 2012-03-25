using System;
using System.Diagnostics;
using System.Xml.Linq;
using Bugfree.SharePoint.Common.Source.Extensions;
using Microsoft.SharePoint;

namespace Bugfree.SharePoint.Common.Source.DataAccess
{
    // See also:
    // http://www.bugfree.dk/blog/2010/01/18/sharepoint-list-access-using-the-repository-pattern/

    public abstract class ListRepository
    {
        protected ListDefinition Definition { get; set; }
        protected SPListItemCollection Result { get; set; }

        protected void Query(SPWeb site, XElement caml)
        {
            AssertValidSite(site);
            AssertListExistence(site);
            AssertListDefinitionSetBySubclass();

            var watch = new Stopwatch();
            var list = site.Lists.TryGetListByInternalName(Definition.ListName);
            var query = new SPQuery { Query = caml.ToString() };
            Debug.WriteLine(
                string.Format("About to run query against list '{0}': {1}", list, caml));
            watch.Start();
            Result = list.GetItems(query);
            watch.Stop();
            Debug.WriteLine(
                string.Format("Query against '{0}' returned {1} rows in {2} ms",
                              list, Result.Count, watch.ElapsedMilliseconds));
        }

        protected void AssertListExistence(SPWeb site)
        {
            if (site.Lists.TryGetListByInternalName(Definition.ListName) == null)
                throw new ArgumentException(
                    string.Format("No '{0}' list on site '{1}'", Definition.ListName, site.Url));
        }

        protected void AssertListDefinitionSetBySubclass()
        {
            if (Definition == null)
                throw new NullReferenceException(
                    string.Format(
                        "Sublcass must set Definition property prior querying '{0}' list",
                        Definition.ListName));
        }

        protected void AssertValidSite(SPWeb site)
        {
            if (site == null)
                throw new NullReferenceException("Site must not be null");
        }
    }
}