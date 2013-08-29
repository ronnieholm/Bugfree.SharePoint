using System;
using System.Collections.Generic;
using Microsoft.SharePoint;

namespace Bugfree.SharePoint.Common.Source.Extensions
{
    // See also:
    // http://www.bugfree.dk/blog/2011/11/15/handy-sharepoint-2010-extension-methods-for-list-definitions/

    public static class SPFieldCollectionExtensions
    {
        // we don't want to call this method CreateNewField because there's already a non-generic field with a
        // similar signature. One would have to remember to add the type argument to use this method instead. By 
        // changing the name slightly we'll get a compiler error if the type argument is missing or cannot 
        // be inferred.
        public static TSPField CreateField<TSPField>(this SPFieldCollection fields, string internalName, string displayName) where TSPField : SPField
        {
            var spFieldToFieldType = new Dictionary<Type, SPFieldType>
            {
                { typeof(SPFieldDateTime), SPFieldType.DateTime },
                { typeof(SPFieldNumber), SPFieldType.Number },
                { typeof(SPFieldUser), SPFieldType.User },
                { typeof(SPFieldBoolean), SPFieldType.Boolean },
                { typeof(SPFieldMultiLineText), SPFieldType.Note },
                { typeof(SPFieldText), SPFieldType.Text },
                { typeof(SPFieldChoice), SPFieldType.Choice },
                { typeof(SPFieldUrl), SPFieldType.URL },
                { typeof(SPFieldMultiChoice), SPFieldType.MultiChoice },
                {typeof (SPFieldGuid), SPFieldType.Guid}
            };

            var fieldType = spFieldToFieldType[typeof(TSPField)]; 
            var list = fields.List;

            // when multiple content types are associated with the list, the field will get added to
            // the 'first' one. A copy of each content type is made when it's associated with a list.
            var fieldName = list.Fields.Add(internalName, fieldType, false);
            fieldName = fieldName.Replace("_x00f8_", "ø");

            var f = list.Fields[fieldName];
            f.Title = displayName;
            f.Update();
            return f as TSPField;
        }

        public static TSPField CreateField<TSPField>(this SPFieldCollection fields, string internalName, string displayName, Action<TSPField> setAdditionalProperties) where TSPField : SPField
        {
            var newField = CreateField<TSPField>(fields, internalName, displayName);
            setAdditionalProperties(newField);
            newField.Update();
            return newField;
        }

        public static TSPField CreateLookup<TSPField>(this SPFieldCollection fields, string lookupListName, string internalName, string displayName) where TSPField : SPFieldLookup
        {
            var currentList = fields.List;
            var lookupList = currentList.ParentWeb.Lists.TryGetListByInternalName(lookupListName);
            var newField = currentList.Fields[currentList.Fields.AddLookup(internalName, lookupList.ID, false)];
            newField.Title = displayName;
            newField.Update();
            return newField as TSPField;
        }

        public static TSPField CreateLookup<TSPField>(this SPFieldCollection fields, string lookupListName, string internalName, string displayName, Action<TSPField> setAdditionalProperties) where TSPField : SPFieldLookup
        {
            var newField = CreateLookup<TSPField>(fields, lookupListName, internalName, displayName);
            setAdditionalProperties(newField);
            newField.Update();
            return newField;
        }
    }
}