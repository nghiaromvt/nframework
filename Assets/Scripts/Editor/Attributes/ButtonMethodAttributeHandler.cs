using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NFramework.Editors
{
    public class ButtonMethodAttributeHandler
    {
        public readonly List<(MethodInfo Method, string Name, ButtonMethodAttribute.EDrawOrder order)> targetMethods;
        private readonly Object _target;

        public int Amount => targetMethods?.Count ?? 0;

        public ButtonMethodAttributeHandler(Object target)
        {
            _target = target;

            var type = target.GetType();
            var bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var members = type.GetMembers(bindings).Where(IsButtonMethod);

            foreach (var member in members)
            {
                var method = member as MethodInfo;
                if (method == null) 
                    continue;

                if (IsValidMember(method, member))
                {
                    var attribute = (ButtonMethodAttribute)Attribute.GetCustomAttribute(method, typeof(ButtonMethodAttribute));
                    if (targetMethods == null) targetMethods = new List<(MethodInfo, string, ButtonMethodAttribute.EDrawOrder)>();
                    targetMethods.Add((method, method.Name.SplitCamelCase(), attribute.drawOrder));
                }
            }
        }

        public void OnBeforeInspectorGUI()
        {
            if (targetMethods == null) 
                return;

            foreach (var method in targetMethods)
            {
                if (method.order != ButtonMethodAttribute.EDrawOrder.BeforeInspector) 
                    continue;

                if (GUILayout.Button(method.Name)) InvokeMethod(_target, method.Method);
            }

            EditorGUILayout.Space();
        }

        public void OnAfterInspectorGUI()
        {
            if (targetMethods == null) return;
            EditorGUILayout.Space();

            foreach (var method in targetMethods)
            {
                if (method.order != ButtonMethodAttribute.EDrawOrder.AfterInspector) 
                    continue;

                if (GUILayout.Button(method.Name)) 
                    InvokeMethod(_target, method.Method);
            }
        }

        public void Invoke(MethodInfo method) => InvokeMethod(_target, method);

        private void InvokeMethod(Object target, MethodInfo method)
        {
            var result = method.Invoke(target, null);

            if (result != null)
            {
                var message = $"{result} \nResult of Method '{method.Name}' invocation on object {target.name}";
                Logger.Log(message, target);
            }
        }

        private bool IsButtonMethod(MemberInfo memberInfo) => Attribute.IsDefined(memberInfo, typeof(ButtonMethodAttribute));

        private bool IsValidMember(MethodInfo method, MemberInfo member)
        {
            if (method == null)
            {
                Logger.Log($"Property <color=brown>{member.Name}</color>.Reason: Member is not a method but has EditorButtonAttribute!");
                return false;
            }

            if (method.GetParameters().Length > 0)
            {
                Logger.Log($"Method <color=brown>{method.Name}</color>.Reason: Methods with parameters is not supported by EditorButtonAttribute!");
                return false;
            }

            return true;
        }
    }
}
