using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshTilesets
{
    public class Tileset : MonoBehaviour, IEnumerable<Tile>
    {
        public const int TILESET_FLAGS_COUNT = 16;
        
        [HideInInspector]
        [SerializeField] private List<Tile> tiles = new List<Tile>();
        
        [SerializeField] private TilesetFlags[] tilesetFlags = new TilesetFlags[TILESET_FLAGS_COUNT];
        public TilesetFlags[] TilesetFlags => tilesetFlags;
        
        // tile id => Tile
        private Dictionary<int, Tile> tileLookup;
    
        public int NewId
        {
            get
            {
                return tiles.Aggregate(0, (current, tile) => Mathf.Max(current, tile.Id)) + 1;
            }
        }
        
        public void AddTile(Tile tile)
        {
            if (!tiles.Contains(tile))
            {
                tiles.Add(tile);
                tile.GetIdFromTileset(this);
            }
            RefreshLookup();
        }

        public void RemoveTile(Tile tile)
        {
            if (tiles.Contains(tile)) tiles.Remove(tile);
            RefreshLookup();
        }
        
        public void CleanTiles()
        {
            for (var i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] != null && tiles[i].transform.parent == transform) continue;
                tiles.RemoveAt(i--);
            }
            RefreshLookup();
        }

        public void RefreshTileset()
        {
            CleanTiles();
            foreach (Tile tile in GetComponentsInChildren<Tile>())
            {
                if (!tiles.Contains(tile)) tiles.Add(tile);
            }
            RefreshLookup();
        }

        public void RefreshLookup()
        {
            if(tileLookup == null) tileLookup = new Dictionary<int, Tile>();
            tileLookup.Clear();

            foreach (Tile tile in tiles)
            {
                tileLookup.Add(tile.Id, tile);
            }
        }

        public Tile LookupTile(int id)
        {
            if(tileLookup == null || tileLookup.Count != tiles.Count) RefreshLookup();
            return !tileLookup.ContainsKey(id) ? null : tileLookup[id];
        }

        public Tile MatchTile(TileInstance instance)
        {
            // Simple match
            return tiles.FirstOrDefault(instance.Matches);
        }

        public Tile MatchClosestTile(TileInstance instance)
        {
            var minDistance = float.MaxValue;
            Tile closestTile = null;

            foreach (Tile tile in tiles)
            {
                // Check if the tile matches edges and tile flags
                if (!instance.MatchesEdges(tile, out var rotation) || !instance.MatchesTileFlags(tile)) continue;
                
                // Compute "distance" between tile dimensions and check if the tile is closer
                var distance = (tile.Size - instance.RotatedSize(rotation)).SqrMagnitude();
                if (!(distance < minDistance)) continue;

                instance.matchedRotation = rotation;
                minDistance = distance;
                closestTile = tile;
            }

            return closestTile;
        }
        
        public IEnumerator<Tile> GetEnumerator()
        {
            return tiles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [System.Serializable]
    public class TilesetFlags
    {
        public string name;
        public bool isToggle = false;
        public string[] options;

        public bool IsEnabled => !string.IsNullOrEmpty(name) && (isToggle || (options != null && options.Length > 0));
        public string[] OptionsWithUndefined => new[] {"Undefined"}.Concat(options).ToArray();
    }

    [System.Serializable]
    public class TilesetFlagsMask
    {
        public int[] flags;
        public bool IsUndefined => flags == null || flags.Length != Tileset.TILESET_FLAGS_COUNT;
        
        public TilesetFlagsMask()
        {
            flags = new int[Tileset.TILESET_FLAGS_COUNT];
        }

        public TilesetFlagsMask(Color[] vertexColors)
        {
            FromVertexColors(vertexColors);
        }

        public int this[int index]
        {
            get
            {
                if (IsUndefined) return 0;
                return flags[index];
            }
            set
            {
                if(IsUndefined && value == 0) return;
                if(IsUndefined) flags = new int[Tileset.TILESET_FLAGS_COUNT];

                flags[index] = value;
            }
        }
        
        public Color[] AsVertexColors
        {
            get
            {
                if (IsUndefined) return null;

                var colors = new Color[Tileset.TILESET_FLAGS_COUNT / 4];
                ToVertexColors(colors);
                return colors;
            }
        }

        public void ToVertexColors(Color[] colors)
        {
            if (IsUndefined)
            {
                for (int i = 0; i < colors.Length; i++) colors[i] = Color.clear;
                return;
            }

            for(int i = 0; i < flags.Length; i += 4)
                colors[i / 4] = new Color(flags[i + 0], flags[i + 1], flags[i + 2], flags[i + 3]);
        }
        
        public void FromVertexColors(Color[] colors)
        {
            // Make sure color array is valid
            if (colors == null || colors.Length * 4 != Tileset.TILESET_FLAGS_COUNT) return;

            // If color array is undefined (everything is 0) and we don't already have a flags array
            // we don't need to create one 
            if(AreVertexColorsUndefined(colors) && flags == null) return;
            
            // Initialize a flags array if we don't have one
            if(flags == null) flags = new int[Tileset.TILESET_FLAGS_COUNT];
            
            for (int i = 0; i < colors.Length; i++)
            {
                var c = colors[i];
                flags[i * 4 + 0] = (int)c.r;
                flags[i * 4 + 1] = (int)c.g;
                flags[i * 4 + 2] = (int)c.b;
                flags[i * 4 + 3] = (int)c.a;
            }
        }

        public static bool AreVertexColorsUndefined(Color[] vertexColors)
        {
            for (int i = 0; i < vertexColors.Length; i++)
            {
                if (vertexColors[i] != Color.clear) return false;
            }

            return true;
        }

        public bool Matches(TilesetFlagsMask other)
        {
            if (other.IsUndefined) return true;

            for (int i = 0; i < other.flags.Length; i++)
            {
                if(other.flags[i] == 0) continue;
                if (other.flags[i] != this[i]) return false;
            }

            return true;
        }

        public void Copy(TilesetFlagsMask other)
        {
            if (other.IsUndefined)
            {
                flags = null;
                return;
            }
            else if(this.IsUndefined) flags = new int[Tileset.TILESET_FLAGS_COUNT];
            other.flags.CopyTo(flags, 0);
        }
    }
}
