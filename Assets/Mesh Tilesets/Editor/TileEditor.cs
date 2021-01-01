using System;
using MeshTilesets;
using UnityEditor;
using UnityEngine;

namespace MeshTilesetsEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Tile))]
    public class TileEditor : Editor
    {
        private Tile Target => target as Tile;
        private Tile[] Targets => targets as Tile[];
        private bool tilesetFlagsFoldout = false;

        private bool utilityFoldout;
        
        public override void OnInspectorGUI()
        {
            if (!Target.Tileset)
            {
                Tileset targetTileset = Target.transform.parent ? Target.transform.parent.GetComponent<Tileset>() : null;
                if (!targetTileset)
                {
                    EditorGUILayout.HelpBox("A Tileset component is required on the parent object.", MessageType.Warning);
                    return;
                }

                Target.Tileset = targetTileset;
            }
            
            base.OnInspectorGUI();
            TilesetFlagsMaskDrawer.DrawTilesetFlagsMask(new GUIContent("Tileset Flags"), Target.tilesetFlags, Target.Tileset, ref tilesetFlagsFoldout);
            if(GUI.changed) EditorUtility.SetDirty(Target);
            
            utilityFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(utilityFoldout, "Utilities");
            if (utilityFoldout)
            {
                if (GUILayout.Button("Size from bounds")) DoAll(t => t.SetBoundsFromChildren());
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Rotate X")) DoAll(t => t.RotateWithoutChildren(Quaternion.Euler(new Vector3(90, 0, 0))));
                if(GUILayout.Button("Rotate Y")) DoAll(t => t.RotateWithoutChildren(Quaternion.Euler(new Vector3(0, 90, 0))));
                if(GUILayout.Button("Rotate Z")) DoAll(t => t.RotateWithoutChildren(Quaternion.Euler(new Vector3(0, 0, 90))));
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DoAll(Action<Tile> operation)
        {
            foreach (var t in targets)
            {
                operation(t as Tile);
            }
        }
    }
}
