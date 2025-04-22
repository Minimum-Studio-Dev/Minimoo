using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using Minimoo.Attributes;

namespace Minimoo.Attributes.Editor
{
    [CustomEditor(typeof(ScriptableObject), true)]
    public class ButtonAttributeDrawer : UnityEditor.Editor
    {
        private Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var targetObject = target;
            var methods = targetObject.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var buttonAttribute = method.GetCustomAttribute<ButtonAttribute>();
                if (buttonAttribute != null)
                {
                    var buttonName = string.IsNullOrEmpty(buttonAttribute.ButtonName) ? method.Name : buttonAttribute.ButtonName;

                    EditorGUILayout.Space();
                    if (GUILayout.Button(buttonName))
                    {
                        method.Invoke(targetObject, null);
                    }
                }
            }
        }
    }
}