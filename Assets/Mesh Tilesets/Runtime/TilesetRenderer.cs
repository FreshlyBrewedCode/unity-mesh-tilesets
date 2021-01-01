using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MeshTilesets
{
    [ExecuteInEditMode]
    public class TilesetRenderer : MonoBehaviour
    {
        [SerializeField] private Tileset tileset;
        public Tileset Tileset => tileset;

        [Tooltip("What method should be used to decide if a tile matches or not in regards to its size")]
        [SerializeField] private SizeMatchMode tileSizeMatch = SizeMatchMode.Exact;
        public SizeMatchMode TileSizeMatch => tileSizeMatch;

        [Tooltip("Tells the TilesetRenderer to pick tiles that require the least amount of rotations to match")]
        [SerializeField] private bool preferFewerRotations;

        [Tooltip("What method should be used to determine the top edge of a tile")]
        [SerializeField] private TileOrientation tileOrientation;
        public TileOrientation Orientation => tileOrientation;
        
        private ProBuilderMesh mesh;
        public ProBuilderMesh Mesh
        {
            get
            {
                if(mesh == null) mesh = GetComponent<ProBuilderMesh>();
                return mesh;
            }
        }

        private TilePool tilePool;
        public TilePool TilePool
        {
            get
            {
                if (tileset == null) return null;
                if(tilePool == null) tilePool = new TilePool(tileset, transform);
                return tilePool;
            }
        }

        // Hidden mesh material
        private static Material tilesetMeshMaterial;
        private static Material TilesetMeshMaterial
        {
            get
            {
                if (tilesetMeshMaterial == null)
                {
                    tilesetMeshMaterial = new Material(Shader.Find("Standard"));
                    tilesetMeshMaterial.name = "HiddenTilsetMesh";
                    tilesetMeshMaterial.color = Color.clear;
                    tilesetMeshMaterial.hideFlags = HideFlags.HideAndDontSave;
                    tilesetMeshMaterial.SetOverrideTag("RenderType", "TransparentCutout");
                    tilesetMeshMaterial.EnableKeyword("_ALPHATEST_ON");
                    tilesetMeshMaterial.renderQueue = 2450;
                    tilesetMeshMaterial.SetFloat("_Mode", 1);
                }
                // tilesetMeshMaterial.EnableKeyword("_ALPHATEST_ON");
                
                return tilesetMeshMaterial;
            }
        }
        
        // Lookups
        // vertex index => shared vertex index
        private Dictionary<int, int> sharedVertexLookup;
        
        // shared vertex index => list of face indices
        private Dictionary<int, List<int>> faceLookup;
        
        // face index => tile index
        private Dictionary<int, int> tileLookup;
        
        // current list of tile instances (not serialized)
        private List<TileInstance> tiles;
        public ReadOnlyCollection<TileInstance> Tiles => new ReadOnlyCollection<TileInstance>(tiles);
        
        // Settings used for tile visualization
        public VisualizationSettings Visualization { get; set; } = VisualizationSettings.Default;

        private void OnEnable()
        {
#if UNITY_EDITOR
            ApplyHiddenMaterial();          
#endif
        }

        private void Awake()
        {
            mesh = GetComponent<ProBuilderMesh>();
            
            // Disable mesh renderer when playing
            if (Application.isPlaying)
            {
                var meshRenderer = GetComponent<MeshRenderer>();
                if(meshRenderer != null) meshRenderer.enabled = false;
            }
        }

        private void OnDestroy()
        {
            SetTileVisibility(true);
        }

        private void OnDrawGizmosSelected()
        {
            if(tiles == null) return;

#if UNITY_EDITOR
            var m = Handles.matrix;
            Handles.matrix = transform.localToWorldMatrix;
            var mg = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            foreach (TileInstance t in tiles)
            {
                var selected = Mesh.selectedFaceIndexes.Contains(t.FaceIndex);
                Handles.color = selected ? Color.yellow : (Color)new Color32(94, 119, 155, 255);
                Gizmos.color = Handles.color;
                
                // Color tileColor = t.IsRect ? new Color(0, 1, 0, 0.2f) : new Color(1, 1, 1, 0.1f);
                Color tileColor = t.IsRect ? new Color(1, 1, 1, 0.15f) : new Color(1, 0, 0, 0.05f);
                Handles.DrawSolidRectangleWithOutline(t.vertices, tileColor, Color.clear);
                
                Handles.DrawAAPolyLine(3f, 
                    t.vertices[0], 
                    t.vertices[1],
                    t.vertices[2],
                    t.vertices[3],
                    t.vertices[0]);
                
                // Normal
                if(Visualization.drawNormals)
                    Gizmos.DrawRay(t.position, t.normal * 0.2f);
                
                // Edge flags
                if (Visualization.drawEdgeFlags)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var p1 = t.vertices[i];
                        var p2 = t.vertices[i + 1 >= 4 ? 0 : i + 1];
                        Gizmos.color = new Color32(94, 119, 155, 255);
                        if (t.edgeFlags[i] == EdgeFlag.ConcaveUp) Gizmos.color = Color.green;
                        if (t.edgeFlags[i] == EdgeFlag.ConvexDown) Gizmos.color = Color.red;
                        if (EdgeFlags.Match(t.edgeFlags[i], EdgeFlag.AnyTile))
                            Gizmos.DrawSphere((p1 + p2) * 0.5f, 0.02f);
                    }
                }
                
                // Pivot
                if (Visualization.drawPivot)
                {
                    var up = t.vertices[1] - t.vertices[0];
                    var right = t.vertices[3] - t.vertices[0];
                    var s = Mathf.Min(up.magnitude, right.magnitude);
                    up.Normalize();
                    right.Normalize();

                    var p = t.vertices[0] + up * s * 0.05f + right * s * 0.05f;
                    Handles.color = Color.green;
                    Handles.ArrowHandleCap(0, p, Quaternion.LookRotation(up), s * 0.1f, EventType.Repaint);
                    Handles.color = Color.red;
                    Handles.ArrowHandleCap(0, p, Quaternion.LookRotation(right), s * 0.1f, EventType.Repaint);
                }
                
                Gizmos.color = Color.white;
                GUI.color = Color.white;

                // Debug vertex colors and indices
                // var quad = t.Face.ToQuad();
                // for (int i = 0; i < 4; i++)
                // {
                //     var color = (Vector4) Mesh.colors[quad[i]];
                //     GUI.color = color == Vector4.zero ? Color.white : Color.red;
                //     var text = $"{quad[i]} {color}";
                //     Handles.Label(t.position + (t.vertices[i] - t.position) * 0.8f, text);
                // }
            }
            
            Handles.color = Color.white;
            Handles.matrix = m;
            Gizmos.matrix = mg;
#endif
        }

        private bool IsRect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            Vector3 e1 = (p1 - p2).normalized;
            Vector3 e2 = (p2 - p3).normalized;
            Vector3 e3 = (p3 - p4).normalized;
            Vector3 e4 = (p4 - p1).normalized;
            
            return Vector3.Dot(e1, e3) < -0.99f && Vector3.Dot(e2, e4) < -0.99f;
        }

        private void GetFaceLookup(IList<Face> faces, Dictionary<int, int> shared, Dictionary<int, List<int>> output)
        {
            output.Clear();
            for (int i = 0; i < faces.Count; i++)
            {
                foreach (var vert in faces[i].distinctIndexes)
                {
                    var sharedVert = shared[vert];
                    if (!output.ContainsKey(sharedVert))
                    {
                        output.Add(sharedVert, new List<int>(){i});
                    }
                    else
                    {
                        if(!output[sharedVert].Contains(i)) output[sharedVert].Add(i);
                    }
                }
            }
        }
        
        public void FullRefresh()
        {
            if(tileset == null) return;
            
            // Make sure mesh has vertex colors
            if (Mesh.colors == null || Mesh.colors.Count != Mesh.positions.Count)
            {
                var colorsCount = Mesh.colors?.Count ?? 0;
                var newColors = new List<Color>();
                for(int i = 0; i < Mesh.positions.Count - colorsCount; i++) newColors.Add(Color.clear);
                mesh.colors = mesh.colors == null ? newColors : mesh.colors.Concat(newColors).ToList();
                mesh.Refresh();
            }
            
            RefreshLookups();
            if(tiles == null) tiles = new List<TileInstance>();
            else
            {
                WriteTileFlagsToVertexColors();
                tiles.Clear();
            }
            if(tileLookup == null) tileLookup = new Dictionary<int, int>();
            tileLookup.Clear();

            // Create tiles
            for(int i = 0; i < mesh.faceCount; i++)
            {
                if(!mesh.faces[i].IsQuad()) continue;
                tiles.Add(new TileInstance(this, mesh.faces[i], i, mesh));
                tileLookup.Add(i, tiles.Count-1);
            }
            
            TilePool?.ReturnTiles();
            
            // Tiles can only be refreshed after all tiles have been setup and the lookup is ready
            foreach (var tile in tiles)
            {
                tile.matchedTileInstance = null;
                tile.FullRefresh();
                MatchTile(tile);
                tile.UpdateTileInstance(TilePool);
                tile.UpdateHideGroups();
            }
            
            TilePool?.ClearPool();
        }

        public void Refresh()
        {
            if (tiles == null)
            {
                FullRefresh();
                return;
            }
            
            foreach (var tile in tiles)
            {
                bool changed = tile.RefreshVertices();
                if (changed)
                {
                    MatchTile(tile);
                    tile.UpdateTileInstance(TilePool);
                }
            }
        }

        // public void MatchTile(TileInstance instance)
        // {
        //     Tile matchedTile = null;
        //         
        //     switch (tileSizeMatch)
        //     {
        //         case SizeMatchMode.ClosestMatch:
        //             matchedTile = tileset.MatchClosestTile(instance);
        //             break;
        //         case SizeMatchMode.Exact:
        //             matchedTile = tileset.MatchTile(instance);
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException(nameof(tileSizeMatch), tileSizeMatch, null);
        //     }
        //
        //     instance.matchedTileId = matchedTile ? matchedTile.Id : -1;
        // }

        public void MatchTile(TileInstance instance)
        {
            var minDistance = float.MaxValue;
            var minRotation = int.MaxValue;
            Tile matchedTile = null;

            foreach (Tile tile in tileset)
            {
                // Check if the tile matches edges and tile flags
                if (!instance.MatchesEdges(tile, out var rotation) || !instance.MatchesTileFlags(tile)) continue;
                
                // Use exact matching
                if (tileSizeMatch == SizeMatchMode.Exact)
                {
                    if (!instance.MatchesSize(tile, rotation)) continue;
                    if (!preferFewerRotations)
                    {
                        matchedTile = tile;
                        minRotation = rotation;
                        break;
                    }
                }
                else
                {
                    // Use "closest match" matching
                    // Compute "distance" between tile dimensions and check if the tile is closer
                    var distance = (tile.Size - instance.RotatedSize(rotation)).SqrMagnitude();
                    if (distance > minDistance) continue;

                    if (distance < minDistance)
                    {
                        matchedTile = tile;
                        instance.matchedRotation = rotation;
                        minRotation = rotation;
                        minDistance = distance;
                        continue;
                    }
                    
                    minDistance = distance;
                }
                
                // Check if the current tile was matched with fewer rotations than the current matchedTile
                if (preferFewerRotations)
                {
                    if (rotation < minRotation)
                    {
                        matchedTile = tile;
                        minRotation = rotation;
                    }
                }
            }

            
            instance.SetMatchedTile(matchedTile, minRotation);
        }
        
        public void WriteTileFlagsToVertexColors()
        {
            var flags = new Color[4];
            int vc = Mesh.vertexCount;
            var colors = new Color[vc];
            
            foreach (TileInstance tile in tiles)
            {
                var verts = tile.Face.ToQuad();
                tile.tilesetFlags.ToVertexColors(flags);
                if (verts[0] < vc && verts[1] < vc && verts[2] < vc && verts[3] < vc)
                {
                    colors[verts[0]] = flags[0];
                    colors[verts[1]] = flags[1];
                    colors[verts[2]] = flags[2];
                    colors[verts[3]] = flags[3];   
                }
            }

            mesh.colors = colors.ToList();
        }
        
        public void RefreshLookups()
        {
            if(sharedVertexLookup == null) sharedVertexLookup = new Dictionary<int, int>();
            if(faceLookup == null) faceLookup = new Dictionary<int, List<int>>();
            
            sharedVertexLookup.Clear();
            faceLookup.Clear();
            
            SharedVertex.GetSharedVertexLookup(Mesh.sharedVertices, sharedVertexLookup);
            GetFaceLookup(Mesh.faces, sharedVertexLookup, faceLookup);
        }

        public List<int> LookupFaces(int sharedVertex)
        {
            return faceLookup[sharedVertex];
        }

        public int LookupSharedVertex(int vertex)
        {
            return sharedVertexLookup[vertex];
        }

        public TileInstance LookupTile(int face)
        {
            if (!tileLookup.ContainsKey(face)) return null;
            return tiles[tileLookup[face]];
        }
        
        public void ApplyHiddenMaterial()
        {
            // Assign material
            var renderer = GetComponent<MeshRenderer>();
            if(renderer) renderer.sharedMaterial = TilesetMeshMaterial;
        }

        public void SetTileVisibility(bool visibleInScene)
        {
            // Just show everything
            if (visibleInScene)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.hideFlags = HideFlags.None;
                }
                return;
            }
            
            // Hide only tiles
            foreach (var tile in tiles)
            {
                if(tile.matchedTileInstance == null) continue;
                tile.matchedTileInstance.instance.hideFlags = HideFlags.HideInHierarchy;
            }
        }
        
        public enum SizeMatchMode
        {
            [Tooltip("Consider the tile with the closest width/height")]
            ClosestMatch,
            [Tooltip("Only consider tiles with exact matching width/height (small tolerance)")]
            Exact
        }

        [System.Serializable]
        public struct VisualizationSettings
        {
            public bool drawNormals;
            public bool drawEdgeFlags;
            public bool drawPivot;
            
            public static readonly VisualizationSettings Default = new VisualizationSettings()
            {
                drawNormals = true,
                drawEdgeFlags = false,
                drawPivot = false
            };
        } 
    }

    public enum TileOrientation
    {
        [Tooltip("Tile orientation is entirly based on the order of the vertices of the face")]
        FromVertexOrder,
        [Tooltip("The tile is oriented so that the top edge faces up (in world space)")]
        TopIsWorldUp,
        [Tooltip("The tile is oriented so that the top edge faces up (in object space)")]
        TopIsObjectUp,
        [Tooltip("The tile is oriented so that the top edge faces forward (in world space)")]
        TopIsWorldForward,
        [Tooltip("The tile is oriented so that the top edge faces forward (in object space)")]
        TopIsObjectForward
    }
}

