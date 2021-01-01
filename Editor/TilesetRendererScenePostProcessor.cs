using MeshTilesets;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace MeshTilesetsEditor
{
    public static class TilesetRendererScenePostProcessor
    {
        [PostProcessScene]
        public static void PostProcessScene()
        {
            foreach (TilesetRenderer t in Object.FindObjectsOfType<TilesetRenderer>())
            {
                if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                    return;

                var meshRenderer = t.GetComponent<MeshRenderer>();
                var meshFilter = t.GetComponent<MeshFilter>();
                var proBuilder = t.GetComponent<ProBuilderMesh>();
                
                if(proBuilder) Object.DestroyImmediate(proBuilder);
                if(meshRenderer) Object.DestroyImmediate(meshRenderer);
                if(meshFilter) Object.DestroyImmediate(meshFilter);
                Object.DestroyImmediate(t);
            }
        }
    }
}
