using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace vietlabs.fr2
{
    internal static class FR2_SerializedObjectExtensions
    {
        internal static void IterateObjectReferences(this SerializedObject serializedObject, Action<UnityObject> callback, string profilerSample = null)
        {
            if (serializedObject == null || callback == null) return;
            
            using (new FR2_ProfilerScope(profilerSample))
            {
                SerializedProperty iterator = serializedObject.GetIterator().Copy();
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType != SerializedPropertyType.ObjectReference) continue;
                    if (iterator.objectReferenceValue == null) continue;
                    callback(iterator.objectReferenceValue);
                }
            }
        }
        
        internal static List<UnityObject> CollectObjectReferences(this SerializedObject serializedObject, string profilerSample = null)
        {
            var results = new List<UnityObject>();
            serializedObject.IterateObjectReferences(obj => results.Add(obj), profilerSample);
            return results;
        }
        
        internal static void IterateProperties(this SerializedObject serializedObject, Action<SerializedProperty> callback, 
            Func<SerializedProperty, bool> filter = null, string profilerSample = null)
        {
            if (serializedObject == null || callback == null) return;
            
            using (new FR2_ProfilerScope(profilerSample))
            {
                SerializedProperty iterator = serializedObject.GetIterator().Copy();
                while (iterator.NextVisible(true))
                {
                    if (filter == null || filter(iterator))
                    {
                        callback(iterator.Copy());
                    }
                }
            }
        }
        
        internal static List<SerializedProperty> CollectProperties(this SerializedObject serializedObject, 
            Func<SerializedProperty, bool> filter = null, string profilerSample = null)
        {
            var results = new List<SerializedProperty>();
            serializedObject.IterateProperties(prop => results.Add(prop), filter, profilerSample);
            return results;
        }
        
        internal static List<UnityObject> GetAllObjectReferencesOptimized(this SerializedObject serializedObject)
        {
            return serializedObject.CollectObjectReferences("FR2.SerializedObject.GetAllObjectReferences");
        }
    }
    
    internal static class FR2_ComponentExtensions
    {
        internal static void IterateObjectReferences(this Component component, Action<UnityObject> callback, string profilerSample = null)
        {
            if (component == null || callback == null) return;
            var serializedObject = new SerializedObject(component);
            serializedObject.IterateObjectReferences(callback, profilerSample);
        }
        
        internal static List<UnityObject> CollectObjectReferences(this Component component, string profilerSample = null)
        {
            if (component == null) return new List<UnityObject>();
            var serializedObject = new SerializedObject(component);
            return serializedObject.CollectObjectReferences(profilerSample);
        }
        
        internal static bool IsValidForProcessing(this Component component)
        {
            return component != null && !(component is Transform);
        }
    }
    
    internal static class FR2_GameObjectExtensions
    {
        internal static void IterateAllObjectReferences(this GameObject gameObject, Action<UnityObject> callback, string profilerSample = null)
        {
            if (gameObject == null || callback == null) return;
            
            using (new FR2_ProfilerScope(profilerSample))
            {
                var components = gameObject.GetComponents<Component>();
                foreach (var component in components)
                {
                    component.IterateObjectReferences(callback);
                }
            }
        }
        
        internal static List<UnityObject> CollectAllObjectReferences(this GameObject gameObject, string profilerSample = null)
        {
            var results = new List<UnityObject>();
            gameObject.IterateAllObjectReferences(obj => results.Add(obj), profilerSample);
            return results;
        }
        
        internal static Component[] GetValidComponents(this GameObject gameObject)
        {
            if (gameObject == null) return new Component[0];
            var components = gameObject.GetComponents<Component>();
            var validComponents = new List<Component>();
            foreach (var component in components)
            {
                if (component.IsValidForProcessing())
                {
                    validComponents.Add(component);
                }
            }
            return validComponents.ToArray();
        }
    }
}
