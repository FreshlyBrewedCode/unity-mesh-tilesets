using System.Collections.Generic;
using System.Linq;
using MeshTilesets;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace MeshTilesetsEditor
{
    [CustomEditor(typeof(TilesetRenderer))]
    public class TilesetRendererEditor : Editor
    {
        TilesetRenderer Target => target as TilesetRenderer;
        private int lastFaceCount;
        private int lastVertCount;
        private SceneOverlayWindow tileOverlay;

        private bool debugFoldout;
        private bool selectedTileFoldout;
        private bool visualizationFoldout;

        private bool disabled = false;
        
        private void OnEnable()
        {
            if (Target.Mesh == null)
            {
                disabled = true;
                return;
            }
            
            // Register events
            ProBuilderEditor.afterMeshModification += OnafterMeshModification;
            ProBuilderEditor.selectionUpdated += ProBuilderEditorOnselectionUpdated;

            Target.ApplyHiddenMaterial();

            // Refresh the tileset renderer
            Target.Refresh();
            
            // Initialized the scene overlay window
            tileOverlay = new SceneOverlayWindow(new GUIContent("Tile"), DoTileOverlayUI, Target);
            
            // Load the visualization settings
            var visSettings = EditorPrefs.GetString($"{nameof(MeshTilesets)}.VisualizationSettings", "{}");
            Target.Visualization = JsonUtility.FromJson<TilesetRenderer.VisualizationSettings>(visSettings);
        }
        
        private void OnDisable()
        {
            ProBuilderEditor.afterMeshModification -= OnafterMeshModification;
            ProBuilderEditor.selectionUpdated -= ProBuilderEditorOnselectionUpdated;
            
            // Save the visualization settings
            if(Target != null)
                EditorPrefs.SetString($"{nameof(MeshTilesets)}.VisualizationSettings", JsonUtility.ToJson(Target.Visualization));
        }

        private void OnafterMeshModification(IEnumerable<ProBuilderMesh> meshes)
        {
            if(meshes.Contains(Target.Mesh)) Target.FullRefresh();
        }

        private void ProBuilderEditorOnselectionUpdated(IEnumerable<ProBuilderMesh> obj)
        {
            // Debug.Log("Update");
            
            if (lastFaceCount != Target.Mesh.faceCount || lastVertCount != Target.Mesh.vertexCount)
            {
                lastFaceCount = Target.Mesh.faceCount;
                lastVertCount = Target.Mesh.vertexCount;
                
                Target.FullRefresh();
            }
            else
            {
                Target.Refresh();
            }
        }
        
        public override void OnInspectorGUI()
        {
            if (disabled)
            {
                if (Target.Mesh != null)
                {
                    disabled = false;
                    OnEnable();
                    return;
                }
                
                EditorGUILayout.HelpBox("Tileset Renderer requires a Pro Builder Mesh component. " +
                                        "Create a new Pro Builder Shape and attach a Tileset Renderer to the " +
                                        "resulting GameObject", MessageType.Error);
                return;
            }
            
            // Draw default inspector
            base.OnInspectorGUI();
            if(GUI.changed) Target.FullRefresh();
            
            // Draw selected tile section
            selectedTileFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(selectedTileFoldout, "Selected Tile");
            if (selectedTileFoldout)
            {
                if (Target.Mesh.selectedFaceCount == 1)
                {
                    EditorGUI.indentLevel++;
                    GUI.enabled = false;

                    var tile = Target.LookupTile(Target.Mesh.selectedFaceIndexes[0]);
                    if (tile != null)
                    {
                        EditorGUILayout.ObjectField("Tile Instance",
                            tile.matchedTileInstance != null ? tile.matchedTileInstance.instance : null,
                            typeof(GameObject),
                            false);

                        EditorGUILayout.Toggle("Flags Undefined", tile.tilesetFlags.IsUndefined);
                        GUI.enabled = true;

                        var foldout = true;
                        GUI.changed = false;
                        TilesetFlagsMaskDrawer.DrawTilesetFlagsMask(new GUIContent("Flags"), tile.tilesetFlags,
                            Target.Tileset, ref foldout);
                        if (GUI.changed) Target.WriteTileFlagsToVertexColors();
                    }

                    GUI.enabled = true;
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("Select a face from the tile mesh", MessageType.Info);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Draw visualization section
            visualizationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(visualizationFoldout, "Visualization");
            if(visualizationFoldout) DoVisualizationSettings();
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Draw debug section
            debugFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(debugFoldout, "Debug");
            if (debugFoldout)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear Pool"))
                {
                    Target.TilePool.ReturnTiles();
                    Target.TilePool.ClearPool();
                }
                if(GUILayout.Button("Rebuild Pool")) Target.TilePool.RebuildPool();
                if(GUILayout.Button("Full Refresh")) Target.FullRefresh();
                
                // Toggle visibility
                var firstChild = Target.transform.childCount > 0 ? Target.transform.GetChild(0) : null;
                if (!firstChild) GUI.enabled = false;
                var visible = firstChild ? firstChild.gameObject.hideFlags == HideFlags.None : false;
                if(GUILayout.Button((visible ? "Hide" : "Show") + " Tiles")) Target.SetTileVisibility(!visible);
                GUI.enabled = true;
                
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void OnSceneGUI()
        {
            if(disabled) return;
            tileOverlay.ShowWindow();
        }

        private static void DoTileOverlayUI(UnityEngine.Object target, SceneView sceneView)
        {
            var Target = target as TilesetRenderer;

            if (Target.Mesh.selectedFaceCount >= 1)
            {
                var tile = Target.LookupTile(Target.Mesh.selectedFaceIndexes[0]);
                if (tile != null)
                {
                    var labelSize = EditorGUIUtility.labelWidth;
                    var fieldSize = EditorGUIUtility.fieldWidth;
                    EditorGUIUtility.labelWidth = 120f;
                    EditorGUIUtility.fieldWidth = 100f;
                    GUI.changed = false;
                    TilesetFlagsMaskDrawer.DrawTilesetFlagsMask(new GUIContent("Flags"), tile.tilesetFlags, Target.Tileset);
                    if (GUI.changed)
                    {
                        if (Target.Mesh.selectedFaceCount > 1)
                        {
                            for (int i = 1; i < Target.Mesh.selectedFaceCount; i++)
                            {
                                var t = Target.LookupTile(Target.Mesh.selectedFaceIndexes[i]);
                                if (t != null) t.tilesetFlags.Copy(tile.tilesetFlags);
                            }
                        }
                        
                        Target.WriteTileFlagsToVertexColors();
                        Target.FullRefresh();
                    }
                    EditorGUIUtility.labelWidth = labelSize;
                    EditorGUIUtility.fieldWidth = fieldSize;
                }
            }
            else
            {
                GUILayout.Label("No tile selected");
            }
        }

        private void DoVisualizationSettings(bool horizontal = false)
        {
            var vis = Target.Visualization;
            DoVisualizationSettings(ref vis);
            Target.Visualization = vis;
        }

        private static void DoVisualizationSettings(ref TilesetRenderer.VisualizationSettings vis)
        {
            vis.drawNormals = EditorGUILayout.ToggleLeft("Normals", vis.drawNormals);
            vis.drawEdgeFlags = EditorGUILayout.ToggleLeft("Edge Flags", vis.drawEdgeFlags);
            vis.drawPivot = EditorGUILayout.ToggleLeft("Orientation", vis.drawPivot);
        }
    }
}
