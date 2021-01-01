using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MeshTilesets
{
    public class TilePool
    {
        public class TileInstanceObject
        {
            public int id;
            public GameObject instance;

            public TileInstanceObject(int id, GameObject instance)
            {
                this.id = id;
                this.instance = instance;
            }
        }
        
        private Tileset tileset;
        private Transform tileParent;
        private Dictionary<int, List<TileInstanceObject>> pool;
        private List<TileInstanceObject> allTiles;
        
        public TilePool(Tileset tileset, Transform tileParent)
        {
            this.tileset = tileset;
            this.tileParent = tileParent;
            RebuildPool();
        }

        public void RebuildPool()
        {
            pool = new Dictionary<int, List<TileInstanceObject>>();
            allTiles = new List<TileInstanceObject>();
            
            // Clear existing untracked instances
            while (this.tileParent.childCount > 0)
            {
                GameObject.DestroyImmediate(tileParent.GetChild(0).gameObject);
            }
            
            // Initialize pool dictionary
            foreach (var tile in tileset)
            {
                pool.Add(tile.Id, new List<TileInstanceObject>());
            }
        }
        
        public TileInstanceObject GetTileInstance(Tile tile) => GetTileInstance(tile.Id);

        public TileInstanceObject GetTileInstance(int id)
        {
            if(!pool.ContainsKey(id)) pool.Add(id, new List<TileInstanceObject>());
        
            var tilePool = pool[id];
            if (tilePool.Count > 0)
            {
                var t = tilePool[tilePool.Count - 1];
                tilePool.Remove(t);
                t.instance.SetActive(true);
                
                return t;
            }

            return CreateTileInstance(id);
        }

        public void ReturnTileInstance(TileInstanceObject tileInstance)
        {
            tileInstance.instance.SetActive(false);
            var tilePool = pool[tileInstance.id];
            if(!tilePool.Contains(tileInstance)) tilePool.Add(tileInstance);
        }
        
        public TileInstanceObject CreateTileInstance(int id)
        {
            var tile = tileset.LookupTile(id);
            var instance = tile.CreateTileInstance();
            instance.transform.SetParent(tileParent);
            
            // Hide objects in editor
            #if UNITY_EDITOR
            // Disable picking so the tile mesh faces can be selected through the tile geometry
            SceneVisibilityManager.instance.DisablePicking(instance, true);
            // Hide the tile objects in the scene view
            instance.hideFlags = HideFlags.HideInHierarchy;
            #endif
            
            var instanceObj = new TileInstanceObject(id, instance);
            allTiles.Add(instanceObj);

            return instanceObj;
        }

        public void ClearPool()
        {
            foreach (var tilePool in pool.Values)
            {
                foreach (TileInstanceObject tile in tilePool)
                {
                    allTiles.Remove(tile);
                    GameObject.DestroyImmediate(tile.instance);
                }
                tilePool.Clear();
            }
        }
        
        public void ReturnTiles()
        {
            foreach (TileInstanceObject t in allTiles)
            {
                ReturnTileInstance(t);
            }
        }
    }
}
