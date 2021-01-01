using System.IO;
using System.Linq;
using MeshTilesets;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MeshTilesetsEditor
{
    [CustomEditor(typeof(Tileset))]
    public class TilesetEditor : Editor
    {
        private Tileset Target => target as Tileset;
        private ReorderableList tileList;

        private bool utilityFoldout;
        
        private void OnEnable()
        {
            // Setup tile list
            var tiles = serializedObject.FindProperty("tiles");
            tileList = new ReorderableList(serializedObject, tiles);
            tileList.drawHeaderCallback = rect => GUI.Label(rect, "Tiles");
            tileList.drawElementCallback = DrawTileListElement;
            tileList.onAddCallback = list =>
            {
                var index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = null;
            };
            tileList.onRemoveCallback = list =>
            {
                // We have to call delete twice to get rid of the element if a tile is assigned
                if(list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue != null)
                    list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            };
        }

        private void DrawTileListElement(Rect rect, int i, bool isactive, bool isfocused)
        {
            var prop = tileList.serializedProperty.GetArrayElementAtIndex(i);
            // GUI.enabled = tileList.list[i] == null;
            GUI.enabled = prop.objectReferenceValue == null;
            // tileList.list[i] = (Tile) EditorGUI.ObjectField(rect.Padding(1f, 2f), tileList.list[i] as Tile, typeof(Tile), true);
            EditorGUI.PropertyField(rect.Padding(1f, 2f), prop, GUIContent.none);
            GUI.enabled = true;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Reorder the tiles to get the desired matching behavior. Tiles further up in " +
                                    "the list will be tested first during the matching process. Because of this tiles " +
                                    "with more specific matching conditions (tile flags, specific edge flags) should be " +
                                    "placed before other more general tiles.", MessageType.Info);
            serializedObject.Update();
            tileList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();

            utilityFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(utilityFoldout, "Utilities");
            if (utilityFoldout)
            {
                if (GUILayout.Button("Tile-ify Children"))
                {
                    CreateTilesFromChildren(Target);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public static void CreateTilesFromChildren(Tileset tileset)
        {
            var children = tileset.transform.Cast<Transform>().ToList();

            foreach (Transform child in children)
            {
                if(child.GetComponent<Tile>() != null) continue;
                
                GameObject newTile = new GameObject(child.gameObject.name);
                newTile.transform.parent = tileset.transform;
                newTile.transform.position = child.position;
                child.SetParent(newTile.transform);

                var tile = newTile.AddComponent<Tile>();
                tile.SetBoundsFromChildren();
                tile.GetIdFromTileset(tileset);
                tileset.AddTile(tile);
            }
        }

        [MenuItem("Assets/Create/Mesh-based Tileset", priority = 120)]
        public static void CreateTilesetAsset()
        {
            var tileset = new GameObject("NewTileset", typeof(Tileset));
            var path = GetSelectedPathOrFallback();

            var uniquePath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{tileset.name}.prefab");
            var prefab = PrefabUtility.SaveAsPrefabAsset(tileset, uniquePath);
            DestroyImmediate(tileset);

            Selection.activeObject = prefab;
        }
        
        [MenuItem("Assets/Create tileset from children", priority = 120)]
        public static void CreateTilesetAssetFromChildren()
        {
            var model = PrefabUtility.InstantiatePrefab(Selection.activeGameObject) as GameObject;
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            var tileset = model.AddComponent<Tileset>();
            CreateTilesFromChildren(tileset);
            model.name = $"{model.name}Tileset";
            tileset.RefreshLookup();
            EditorUtility.SetDirty(tileset);
            
            var path = GetSelectedPathOrFallback();
            var uniquePath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{model.name}.prefab");
            var prefab = PrefabUtility.SaveAsPrefabAsset(model, uniquePath);
            DestroyImmediate(model);

            Selection.activeObject = prefab;
        }
        
        [MenuItem("Assets/Create tileset from children", validate = true)]
        public static bool CanCreateTilesetAssetFromChildren()
        {
            return Selection.activeGameObject != null &&
                   PrefabUtility.GetPrefabAssetType(Selection.activeGameObject) == PrefabAssetType.Model;
        }

        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
  
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }
    }

    public static class RectExtension
    {
        public static Rect Padding(this Rect rect, float vertical, float horizontal)
        {
            return rect.Padding(vertical, vertical, horizontal, horizontal);
        }
        
        public static Rect Padding(this Rect rect, float top, float bottom, float left, float right)
        {
            return new Rect(rect.x + left, rect.y + top, rect.width - left - right, rect.height - top - bottom);
        }

        public static Rect[] SplitX(this Rect rect, params float[] segments)
        {
            var rects = new Rect[segments.Length];

            float start = rect.x;
            for (int i = 0; i < rects.Length; i++)
            {
                rects[i] = new Rect(start, rect.y, rect.width * segments[i], rect.height);
                start = rects[i].xMax;
            }

            return rects;
        }
    }
}


