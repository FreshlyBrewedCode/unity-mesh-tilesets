using MeshTilesets;
using UnityEditor;
using UnityEngine;

namespace MeshTilesetsEditor
{
    [CustomPropertyDrawer(typeof(TilesetFlagsMask))]
    public class TilesetFlagsMaskDrawer : PropertyDrawer
    {
        private bool foldout = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }

        // TODO: implement generic property drawer 
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.targetObject is Component c)
            {
                var tileset = c.gameObject.GetComponentInParent<Tileset>();
                if (tileset != null)
                {
                    // EditorGUI.BeginProperty(position, label, property);
                    var mask = property.GetValue<TilesetFlagsMask>();
                    DrawTilesetFlagsMask(label, mask, tileset, ref foldout);
                    if (GUI.changed) property.SetValue(mask);
                    // EditorGUI.EndProperty();
                    return;
                }
            }
            EditorGUI.HelpBox(position, "No Tileset found", MessageType.None);
        }

        public static void DrawTilesetFlagsMask(GUIContent label, TilesetFlagsMask mask, Tileset tileset, ref bool foldout)
        {
            foldout = EditorGUILayout.Foldout(foldout, label);

            if (foldout)
            {
                EditorGUI.indentLevel++;
                DrawTilesetFlagsMask(label, mask, tileset);
                EditorGUI.indentLevel--;
            }
            // EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public static void DrawTilesetFlagsMask(GUIContent label, TilesetFlagsMask mask, Tileset tileset)
        {
            var flags = tileset.TilesetFlags;
                
            for (int i = 0; i < Tileset.TILESET_FLAGS_COUNT; i++)
            {
                if(!flags[i].IsEnabled) continue;

                if (flags[i].isToggle)
                {
                    mask[i] = EditorGUILayout.Toggle(flags[i].name, mask[i] == 1) ? 1 : 0;
                }
                else
                {
                    mask[i] = EditorGUILayout.Popup(flags[i].name, mask[i], flags[i].OptionsWithUndefined);
                }
            }
        }
    }
}
