using System.Linq;
using MeshTilesets;
using UnityEditor;
using UnityEngine;

namespace MeshTilesetsEditor
{
    [CustomPropertyDrawer(typeof(TilesetFlagsMask))]
    public class TilesetFlagsMaskDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.targetObject is Component c)
            {
                var tileset = c.gameObject.GetComponentInParent<Tileset>();
                if (tileset != null)
                {
                    if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
                    
                    var mask = property.GetValue<TilesetFlagsMask>();
                    return EditorGUIUtility.singleLineHeight + tileset.TilesetFlags.GetDisplayHeight();
                }
            }

            return EditorGUIUtility.singleLineHeight * 2;
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
                    bool foldout = property.isExpanded;
                    DrawTilesetFlagsMask(position, label, mask, tileset, ref foldout);
                    property.isExpanded = foldout;
                    if (GUI.changed) property.SetValue(mask);
                    // EditorGUI.EndProperty();
                    return;
                }
            }
            EditorGUI.HelpBox(position, "No Tileset found", MessageType.None);
        }

        public static void DrawTilesetFlagsMask(Rect pos, GUIContent label, TilesetFlagsMask mask, Tileset tileset, ref bool foldout)
        {
            // Foldout
            pos.height = EditorGUIUtility.singleLineHeight;
            foldout = EditorGUI.Foldout(pos, foldout, label);
            pos.y += pos.height + EditorGUIUtility.standardVerticalSpacing;
            
            // Indent
            pos.x += EditorGUIUtility.singleLineHeight;
            pos.width -= EditorGUIUtility.singleLineHeight;
            
            if (foldout)
            {
                DrawTilesetFlagsMask(pos, label, mask, tileset);
            }
            // EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public static void DrawTilesetFlagsMask(Rect pos, GUIContent label, TilesetFlagsMask mask, Tileset tileset)
        {
            var flags = tileset.TilesetFlags;
                
            for (int i = 0; i < Tileset.TILESET_FLAGS_COUNT; i++)
            {
                if(!flags[i].IsEnabled) continue;

                if (flags[i].isToggle)
                {
                    mask[i] = EditorGUI.Toggle(pos, flags[i].name, mask[i] == 1) ? 1 : 0;
                }
                else
                {
                    mask[i] = EditorGUI.Popup(pos, flags[i].name, mask[i], flags[i].OptionsWithUndefined);
                }
                
                pos.height = EditorGUIUtility.singleLineHeight;
                pos.y += pos.height + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        public static void DrawTilesetFlagsMask(GUIContent label, TilesetFlagsMask mask, Tileset tileset, ref bool foldout)
        {
            float height = EditorGUIUtility.singleLineHeight + (foldout ? tileset.TilesetFlags.GetDisplayHeight() : 0);
            var rect = GUILayoutUtility.GetRect(0, EditorGUIUtility.currentViewWidth, height, height);
            DrawTilesetFlagsMask(rect, label, mask, tileset, ref foldout);
        }

        public static void DrawTilesetFlagsMask(GUIContent label, TilesetFlagsMask mask, Tileset tileset)
        {
            float height = tileset.TilesetFlags.GetDisplayHeight();
            var rect = GUILayoutUtility.GetRect(0, EditorGUIUtility.currentViewWidth, height, height);
            DrawTilesetFlagsMask(rect, label, mask, tileset);
        }
    }

    public static class TilesetFlagsExtension
    {
        public static float GetDisplayHeight(this TilesetFlags[] flags)
        {
            return flags.Aggregate(0f, (r, f) => 
                r + (f.IsEnabled ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing : 0)       
            );
        }
    }
}
