using System.Linq;
using Microsoft.SharePoint;

namespace Bugfree.SharePoint.Common.Source.Extensions
{
    // See also:
    // http://www.bugfree.dk/blog/2011/11/19/adding-event-receivers-to-sharepoint-lists-on-the-fly/

    public static class SPListExtensions
    {
        public static void RegisterEventReceiver<TReceiver>(this SPList list,
                                                            SPEventReceiverType receiverType,
                                                            int sequenceNumber) where TReceiver : SPItemEventReceiver
        {
            var assemblyName = typeof (TReceiver).Assembly.FullName;
            var className = typeof (TReceiver).FullName;

            (from SPEventReceiverDefinition definition in list.EventReceivers
             where definition.Assembly == assemblyName &&
                   definition.Class == className &&
                   definition.Type == receiverType
             select list.EventReceivers[definition.Id])
                .ToList()
                .ForEach(receiverToDelete => receiverToDelete.Delete());

            var receiver = list.EventReceivers.Add();
            receiver.Type = receiverType;
            receiver.Assembly = assemblyName;
            receiver.Class = className;
            receiver.SequenceNumber = sequenceNumber;
            receiver.Update();
            list.Update();
        }
    }
}