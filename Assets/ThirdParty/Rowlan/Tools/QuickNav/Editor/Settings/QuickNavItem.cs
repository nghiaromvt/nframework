using System;
using UnityEditor;
using UnityEngine;

namespace Rowlan.Tools.QuickNav
{
    [Serializable]
    public class QuickNavItem
    {
        public static QuickNavItem CreateSeparator()
        {
            QuickNavItem item = new QuickNavItem(null, false);

            item.context = Context.Separator;
            item.title = "<Separator>";

            return item;
        }

        public enum Context
        {
            Scene,
            Project,
            Separator
        }

        /// <summary>
        /// Whether the selection came from the scene or the project
        /// </summary>
        public Context context;

        /// <summary>
        /// Associated text to this item.
        /// For separators this is the separator title.
        /// For other context this is currently unused, the gameobject itself is displayed in that case.
        /// </summary>
        public string title;

        /// <summary>
        /// The selection instance id
        /// </summary>
        //public int instanceId;

        /// <summary>
        /// The name of the selected object
        /// </summary>
        public UnityEngine.Object unityObject;

        /// <summary>
        /// the 
        /// </summary>
        public string objectGuid;

        public QuickNavItem(UnityEngine.Object unityObject, bool isProjectContext)
        {
            this.unityObject = unityObject;
            this.context = isProjectContext ? Context.Project : Context.Scene;

            GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(unityObject);
            objectGuid = globalObjectId.ToString();
        }

        /// <summary>
        /// Get the unity object using the object guid and assign it.
        /// This may become necessary e. g. after a restart of the unity editor.
        /// In that case the unity object could be lost, but using the object guid we can restore it.
        /// </summary>
        public void Refresh()
        {
            if (objectGuid == null)
                return;

            if (context == Context.Separator)
                return;

            GlobalObjectId id;
            if (!GlobalObjectId.TryParse( objectGuid, out id))
            {
                Debug.Log("obj is null for " + objectGuid);
                return;
            }

            UnityEngine.Object parsedObject = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);

            if (parsedObject == null)
                return;

            unityObject = parsedObject;
        }
    }
}