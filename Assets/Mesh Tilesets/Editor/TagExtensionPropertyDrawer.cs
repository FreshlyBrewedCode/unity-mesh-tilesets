using MeshTilesets;
using UnityEditor;
using UnityEngine;

namespace MeshTilesetsEditor
{
    [CustomPropertyDrawer(typeof(Tag))]
    public class TagExtensionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var tagName = property.FindPropertyRelative("tagName");
            tagName.stringValue = EditorGUI.TagField(position, label, tagName.stringValue);
            
            EditorGUI.EndProperty();
        }
    }
}