using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MeshTilesets
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnabledIfAttribute : PropertyAttribute
    {
        public string boolFieldName;
        public bool invert;

        public EnabledIfAttribute(string boolFieldName, bool invert = false)
        {
            this.boolFieldName = boolFieldName;
            this.invert = invert;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnabledIfAttribute))]
    public class EnabledIfAttributeDrawer : PropertyDrawer
    {
        private EnabledIfAttribute Attribute => attribute as EnabledIfAttribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var enabledProp = property.serializedObject.FindProperty(Attribute.boolFieldName);
            var enabled = enabledProp == null || enabledProp.boolValue;
            var wasEnabled = GUI.enabled;
            GUI.enabled = Attribute.invert ? !enabled : enabled;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = wasEnabled;
        }
    }
#endif
}