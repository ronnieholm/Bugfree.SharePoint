using System;
using Microsoft.SharePoint;

namespace Bugfree.SharePoint.Common.Source.DataAccess
{
    // See also
    // http://www.bugfree.dk/blog/2010/01/11/sharepoint-list-definition-using-the-template-pattern

    public abstract class ListDefinition
    {
        public string ListTitle;
        public string ListDescription;
        public SPListTemplateType ListTemplateType;        
        private string _listName;
        protected SPList List;

        public string ListName
        {
            get
            {
                // assume convention over configuration
                if (string.IsNullOrEmpty(_listName))
                {
                    var derivedClassName = GetType().Name;
                    var classNameWithoutPostfix = derivedClassName.Replace("Definition", "");
                    _listName = classNameWithoutPostfix;                    
                }
                return _listName;
            }
            set { _listName = value; }
        }

        public void CreateOnSite(SPWeb site)
        {
            if (site == null)
                throw new NullReferenceException("Expected valid site");
            if (string.IsNullOrEmpty(ListTitle))
                ListTitle = ListName;
            if (ListTemplateType == SPListTemplateType.NoListTemplate)
                throw new ArgumentException("Expected ListTemplateType != NoListTemplate");

            CreateList(site);
            if (List == null)
                throw new NullReferenceException("Expected list member not null");

            CreateFields(List);
            List.Update();

            UpdateViews(List);
            List.Update();

            SetAdditionalProperties(List);
            List.Update();
        }

        protected virtual void CreateList(SPWeb site)
        {
            var guid = site.Lists.Add(ListName, ListDescription, ListTemplateType);
            List = site.Lists[guid];
            List.Title = ListTitle;
            List.Update();
        }

        protected virtual void CreateFields(SPList l) { }
        protected virtual void UpdateViews(SPList l) { }
        protected virtual void SetAdditionalProperties(SPList l) { }

        public void SetFieldDisplayName(string internalName, string displayName)
        {
            var field = List.Fields.GetFieldByInternalName(internalName);
            field.Title = displayName;
            field.Update();
        }
    }
}