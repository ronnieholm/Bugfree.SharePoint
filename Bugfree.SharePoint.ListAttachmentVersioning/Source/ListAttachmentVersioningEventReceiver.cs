using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.SharePoint;

namespace Bugfree.SharePoint.ListAttachmentVersioning.Source
{
    // Prerequisites:
    // 1. Create a Document Library named ShadowLibrary on the same site as the list to version
    // 2. Add a row named CustomVersion of type string to the list to version
    public class ListAttachmentVersioningEventReceiver : SPItemEventReceiver
    {
        private const string CustomVersion = "CustomVersion";
        private const string ShadowLibrary = "ShadowLibrary";

        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
            SetCustomVersionLabel(properties.ListItem);
            CreateSnapshot(properties);
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
            var item = properties.ListItem;
            if (RollbackHappened(item))
            {
                RestoreSnapshot(properties);
                SetCustomVersionLabel(item);
                CreateSnapshot(properties);
            }
            else
            {
                CreateSnapshot(properties);
                SetCustomVersionLabel(item);
            }
        }

        private void CreateSnapshot(SPItemEventProperties properties)
        {
            using (var site = properties.OpenWeb())
            {
                var item = properties.ListItem;
                var shadowLibrary = site.Lists[ShadowLibrary] as SPDocumentLibrary;
                var path = string.Format("Versions/{0}/{1}", item.ID, GetOfficialVersionLabel(item));
                var shadowFolder = CreateFolderPath(shadowLibrary, path);
                
                foreach (string fileName in item.Attachments)
                {
                    SPFile existingFile = item.ParentList.ParentWeb.GetFile(item.Attachments.UrlPrefix + fileName);
                    SPFile newFile = shadowFolder.Files.Add(fileName, existingFile.OpenBinaryStream());
                    newFile.Item.Update();
                }
            }
        }

        private bool RollbackHappened(SPListItem item)
        {
            var culture = CultureInfo.InvariantCulture;
            var currentVersion = float.Parse(GetOfficialVersionLabel(item), culture);
            var lastVersion = float.Parse(GetCustomVersionLabel(item), culture);
            return currentVersion > lastVersion + 1;
        }

        private void RestoreSnapshot(SPItemEventProperties properties)
        {
            var item = properties.ListItem;
            var restoreVersion = GetCustomVersionLabel(item);
            EventFiringEnabled = false;
            item.Attachments.Cast<string>().ToList().ForEach(attachment => item.Attachments.Delete(attachment));
            
            using (var site = properties.OpenWeb())
            {
                var path = string.Format("Versions/{0}/{1}", item.ID, restoreVersion);
                var shadowLibrary = site.Lists[ShadowLibrary] as SPDocumentLibrary;
                var source = CreateFolderPath(shadowLibrary, path);
                foreach (SPFile file in source.Files) 
                    item.Attachments.Add(file.Name, file.OpenBinary());
            }
            item.SystemUpdate(false);
            EventFiringEnabled = true;
        }

        // can only get folder creation to work with Document Libraries    
        private SPFolder CreateFolderPath(SPDocumentLibrary list, string path)
        {
            return CreateFolderPathRecursive(list.RootFolder, path.Split('/').ToList());
        }

        private SPFolder CreateFolderPathRecursive(SPFolder folder, IList<string> pathComponents)
        {
            if (pathComponents.Count == 0) 
                return folder;
            
            SPFolder newFolder;
            try
            {
                newFolder = folder.SubFolders[pathComponents.First()];
            }
            catch (ArgumentException)
            {
                newFolder = folder.SubFolders.Add(pathComponents.First());
            }
            
            pathComponents.RemoveAt(0);
            return CreateFolderPathRecursive(newFolder, pathComponents);
        }

        private void SetCustomVersionLabel(SPListItem item)
        {
            EventFiringEnabled = false;
            item[CustomVersion] = GetOfficialVersionLabel(item);
            item.SystemUpdate(false);
            EventFiringEnabled = true;
        }

        private string GetCustomVersionLabel(SPItem item)
        {
            return item[CustomVersion] as string;
        }

        private string GetOfficialVersionLabel(SPListItem item)
        {
            return item.Versions[0].VersionLabel;
        }
    }
}