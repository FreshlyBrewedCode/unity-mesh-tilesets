using System;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace MeshTilesets
{
    public class TileInstance
    {
        private const float RECT_THRESHOLD = 0.001f;
        private const float FLAT_THRESHOLD = 0.001f;
        private const float SIZE_MATCH_THRESHOLD = 0.001f;
        
        public Vector3[] vertices;
        public int orientationOffset;
        public EdgeFlags edgeFlags; 
        public Vector3 position;
        public Vector3 normal;
        public float width;
        public float height;
        public TilesetFlagsMask tilesetFlags;

        private int matchedTileId = -1;
        public int matchedRotation = 0;
        public int lastMatchedRotation = 0;
        public TilePool.TileInstanceObject matchedTileInstance;
        
        private Face face;
        private int faceIndex;
        private ProBuilderMesh mesh;
        private TilesetRenderer tilesetRenderer;
        
        private Vector3 lastPos;
        private float lastWidth, lastHeight;

        public int FaceIndex => faceIndex;
        public Face Face => face;
        
        public TileInstance(TilesetRenderer renderer, Face face, int faceIndex, ProBuilderMesh mesh)
        {
            tilesetRenderer = renderer;
            this.face = face;
            this.faceIndex = faceIndex;
            this.mesh = mesh;
            this.edgeFlags = new EdgeFlags();
            this.vertices = new Vector3[4];
            RefreshVertices();
        }
        
        public void FullRefresh()
        {
            RefreshVertices(true);
            RefreshEdgeFlags();
        }

        public bool RefreshVertices(bool refreshOrientation = false)
        {
            var quad = face.ToQuad();
            var colors = new Color[vertices.Length];
            
            if(refreshOrientation) RefreshOrientation(quad, tilesetRenderer.Orientation);
            
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = mesh.positions[quad[(i + orientationOffset) % 4]];
                colors[i] = mesh.colors[quad[i]];
            }
            
            
            var leftEdge = vertices[1] - vertices[0];
            var bottomEdge = vertices[3] - vertices[0];

            height = leftEdge.magnitude;
            width = bottomEdge.magnitude;
            
            this.position = (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4;
            this.normal = Vector3.Cross(leftEdge, bottomEdge).normalized;
            
            // Compute flags
            if(tilesetFlags == null) tilesetFlags = new TilesetFlagsMask(colors);
            else tilesetFlags.FromVertexColors(colors);
            
            // Check if anything has changed
            bool changed = lastPos != position || 
                           Mathf.Abs(lastWidth - width) > SIZE_MATCH_THRESHOLD || 
                           Mathf.Abs(lastHeight - height) > SIZE_MATCH_THRESHOLD;
            
            lastPos = position;
            lastWidth = width;
            lastHeight = height;

            return changed;
        }

        public void RefreshEdgeFlags()
        {
            var quad = face.ToQuad();
            
            // Edge flags
            for (int i = 0; i < 4; i++)
            {
                var v1 = quad[(i + orientationOffset) % 4];
                var v2 = quad[(i + 1 + orientationOffset) % 4];

                var sharedV1 = tilesetRenderer.LookupSharedVertex(v1);
                var sharedV2 = tilesetRenderer.LookupSharedVertex(v2);

                var sharedFace = tilesetRenderer.LookupFaces(sharedV1)
                    .Intersect(tilesetRenderer.LookupFaces(sharedV2))
                    .Where(f => f != this.faceIndex);

                if (sharedFace.Count() != 1)
                {
                    edgeFlags[i] = EdgeFlag.Empty;
                }
                else
                {
                    var face = sharedFace.First();
                    var tile = tilesetRenderer.LookupTile(face);
                    
                    if(tile == null) edgeFlags[i] = EdgeFlag.Empty;
                    else
                    {
                        // Get the "tangent" i.e. a vector pointing in the direction of the edge (left, right, top or bottom)
                        // and is perpendicular to the normal of the tile
                        var tangent = (vertices[i] - vertices[(i + 3) % 4]).normalized; // vertices[i] - vertices[i - 1] 
                        var angle = Vector3.Dot(tangent, tile.normal);
                        // Debug.Log(tile.normal);
                        
                        if (Mathf.Abs(angle) <= FLAT_THRESHOLD) edgeFlags[i] = EdgeFlag.Flat;
                        else if (angle > FLAT_THRESHOLD) edgeFlags[i] = EdgeFlag.ConvexDown;
                        else edgeFlags[i] = EdgeFlag.ConcaveUp;
                    }
                }
            }
        }

        public void RefreshOrientation(int[] indices, TileOrientation orientation)
        {
            if (orientation == TileOrientation.FromVertexOrder)
            {
                orientationOffset = 0;
                return;
            }
            
            // Get the desired direction for the top edge
            Vector3 topTarget;
            switch (orientation)
            {
                case TileOrientation.TopIsWorldUp:
                    topTarget = tilesetRenderer.transform.InverseTransformDirection(Vector3.up);
                    break;
                case TileOrientation.TopIsObjectUp:
                    topTarget = Vector3.up;
                    break;
                case TileOrientation.TopIsWorldForward:
                    topTarget = tilesetRenderer.transform.InverseTransformDirection(Vector3.forward);
                    break;
                case TileOrientation.TopIsObjectForward:
                    topTarget = Vector3.forward;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // We use the dot product to compare the top vector of the tile with the target top direction
            // The dot product is largest if the two vectors are facing in the same direction, so we
            // are looking for the orientation with the maximum dot product
            float maxDot = float.MinValue;
            
            // We try 4 different orientation offsets
            for (int i = 0; i < 4; i++)
            {
                // "top vector" is just the vector pointing from v[0] to v[1]
                var top = (mesh.positions[indices[(1 + i) % 4]] - mesh.positions[indices[(0 + i) % 4]]).normalized;
                var dot = Vector3.Dot(top, topTarget);
                if (dot > maxDot)
                {
                    orientationOffset = i;
                    maxDot = dot;
                }
            }
        }
        
        public bool IsRect 
        {
            get
            {
                Vector3 e1 = (vertices[0] - vertices[1]).normalized;
                Vector3 e2 = (vertices[1] - vertices[2]).normalized;
                Vector3 e3 = (vertices[2] - vertices[3]).normalized;
                Vector3 e4 = (vertices[3] - vertices[0]).normalized;
                float angle1 = Vector3.Dot(e1, e2);
                float angle2 = Vector3.Dot(e3, e4);

                return Vector3.Dot(e1, e3) < (-1 + RECT_THRESHOLD) && Vector3.Dot(e2, e4) < (-1 + RECT_THRESHOLD) &&
                    Mathf.Abs(angle1) < RECT_THRESHOLD && Mathf.Abs(angle2) < RECT_THRESHOLD;
            }
        }

        public bool Matches(Tile tile)
        {
            int rotation;
            if (!MatchesEdges(tile, out rotation)) return false;
            bool matches = MatchesSize(tile, rotation) && MatchesTileFlags(tile);

            if (matches)
            {
                matchedRotation = rotation;
            }
            
            return matches;
        }
        
        public bool MatchesEdges(Tile tile, out int rotation)
        {
            var matches = tile.EdgeFlags.Matches(edgeFlags, true, out lastMatchedRotation);
            rotation = lastMatchedRotation;
            return matches;
        }

        public bool MatchesSize(Tile tile, int rotation)
        {
            bool invert = rotation % 2 == 1;
            return (Mathf.Abs(tile.Width - (invert ? height : width)) <= SIZE_MATCH_THRESHOLD &&
                           Mathf.Abs(tile.Height - (invert ? width : height)) <= SIZE_MATCH_THRESHOLD);
        }

        public bool MatchesTileFlags(Tile tile)
        {
            return tilesetFlags.Matches(tile.tilesetFlags);
        }
        
        public void SetMatchedTile(Tile tile, int matchedRotation)
        {
            if (tile != null && tile.Id != matchedTileId && tile.OverrideOrientation)
            {
                var quads = face.ToQuad();
                RefreshOrientation(quads, tile.Orientation);
                RefreshVertices();
            }
            matchedTileId = tile ? tile.Id : -1;
            this.matchedRotation = matchedRotation;
        }

        public Vector2 RotatedSize(int rotation)
        {
            return rotation % 2 == 0
                ? new Vector2(width, height)
                : new Vector2(height, width);
        }
        
        public void UpdateTileInstance(TilePool pool)
        {
            // Check if we need a new tile instance
            if (matchedTileInstance == null || matchedTileInstance.id != matchedTileId)
            {
                if(matchedTileInstance != null) pool.ReturnTileInstance(matchedTileInstance);
                if (matchedTileId == -1) return;

                matchedTileInstance = pool.GetTileInstance(matchedTileId);
            }
            
            // Align tile instance
            var tileTransform = matchedTileInstance.instance.transform;
            tileTransform.localPosition = position;

            var forward = (vertices[1] - vertices[0]).normalized;
            tileTransform.localRotation = Quaternion.AngleAxis(-90 * matchedRotation, normal) * Quaternion.LookRotation(forward, normal);
        }

        public void UpdateHideGroups()
        {
            if (matchedTileInstance != null)
            {
                tilesetRenderer.Tileset.LookupTile(matchedTileId).ApplyHideGroups(this, matchedTileInstance.instance.transform);
            }
        }
    }
}
