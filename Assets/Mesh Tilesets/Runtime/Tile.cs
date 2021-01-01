using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MeshTilesets
{
    public enum EdgeFlag
    {
        Any = 0, // Matches with everything
        Empty = 1, // Matches if edge has no adjacent tiles
        AnyTile = 2, // Matches if edge has any adjacent tiles
        Flat = 3, // Matches if edge has an adjacent tile with the same normal
        ConvexDown = 4, // Matches if edge has an adjacent tile that bends down (convex geometry)
        ConcaveUp = 5, // Matches if edge has an adjacent tile that bends up (concave geometry)
        NotFlat = 6, // NotFlat = Flat * 2
        NotConvexDown = 8,
        NotConcaveUp = 10
    }
    
    [System.Serializable]
    public class EdgeFlags
    {
        public EdgeFlag top = EdgeFlag.Any;
        public EdgeFlag bottom = EdgeFlag.Any;
        public EdgeFlag left = EdgeFlag.Any;
        public EdgeFlag right = EdgeFlag.Any;

        public override int GetHashCode()
        {
            return ((int) top << 0) + ((int)bottom << 3) + ((int)left << 6) + ((int)right << 9);
        }

        public static bool NotFlag(EdgeFlag a, EdgeFlag b)
        {
            if ((int) a >= 6) return (EdgeFlag) ((int) a / 2) != b;
            return false;
        }
        
        public static bool Match(EdgeFlag a, EdgeFlag b)
        {
            return a == b || a == EdgeFlag.Any || b == EdgeFlag.Any ||
                   (a == EdgeFlag.AnyTile && b > EdgeFlag.Empty && b <= EdgeFlag.ConcaveUp) ||
                   (b == EdgeFlag.AnyTile && a > EdgeFlag.Empty && a <= EdgeFlag.ConcaveUp) ||
                   NotFlag(a, b) || NotFlag(b, a);
        }

        public bool Matches(EdgeFlags other, bool allowRotation, out int rotation)
        {
            rotation = 0;
            
            // Simple match of all edges
            if (!allowRotation)
                return Match(top, other.top) &&
                       Match(bottom, other.bottom) &&
                       Match(left, other.left) &&
                       Match(right, other.right);
            
            // Try to match any rotation
            for (int i = 0; i < 4; i++)
            {
                rotation = i;
                if (this.RotatedClockwise(i).Matches(other)) return true;
            }
            
            return false;
        }

        public bool Matches(EdgeFlags other, bool allowRotation = false)
        {
            return Matches(other, allowRotation, out _);
        }

        public EdgeFlag this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return left;
                    case 1: return top;
                    case 2: return right;
                    case 3: return bottom;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0: 
                        left = value;
                        break;
                    case 1: 
                        top = value;
                        break;
                    case 2: 
                        right = value;
                        break;
                    case 3: 
                        bottom = value;
                        break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public EdgeFlags RotatedClockwise(int numberOfRotations)
        {
            return new EdgeFlags
            {
                left = this[(numberOfRotations + 0) % 4],
                top = this[(numberOfRotations + 1) % 4],
                right = this[(numberOfRotations + 2) % 4],
                bottom = this[(numberOfRotations + 3) % 4]
            };
        }
    }
    
    [ExecuteInEditMode]
    public class Tile : MonoBehaviour
    {
        const float GIZMO_MARGIN = 0.25f;

        [SerializeField] private int id = -1;
        [SerializeField] private float width = 1;
        [SerializeField] private float height = 1;
        [SerializeField] private bool overrideOrientation = false;
        [EnabledIf(nameof(overrideOrientation))]
        [SerializeField] private TileOrientation orientation;
        [SerializeField] private EdgeFlags edgeFlags;
        [SerializeField][HideInInspector] public TilesetFlagsMask tilesetFlags;
        [SerializeField] private TileHideGroup[] hideGroups;

        private Tileset tileset;
        public Tileset Tileset
        {
            get => tileset;
            set
            {
                if(tileset) tileset.RemoveTile(this);
                tileset = value;
                tileset.AddTile(this);
            }
        }

        public int Id => id;
        public float Width => width;
        public float Height => height;
        public bool OverrideOrientation => overrideOrientation;
        public TileOrientation Orientation => orientation;
        public EdgeFlags EdgeFlags => edgeFlags;

        public Vector2 Size => new Vector2(width, height);

        public void GetIdFromTileset(Tileset tileset)
        {
            id = tileset.NewId;
        }

        private void OnDestroy()
        {
            if(tileset) tileset.RemoveTile(this);
        }

        public GameObject CreateTileInstance()
        {
            GameObject instance = new GameObject(gameObject.name);
            bool isPrefab = false;
            foreach (Transform child in transform)
            {
                GameObject childInstance;
#if UNITY_EDITOR
                var prefabInstance = PrefabUtility.GetNearestPrefabInstanceRoot(child);
                if (prefabInstance != null && prefabInstance.gameObject == child.gameObject)
                {
                    string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabInstance);
                    var prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
                    childInstance = PrefabUtility.InstantiatePrefab(prefab, instance.transform) as GameObject;
                    isPrefab = true;
                }
                else
                {
#endif
                    childInstance = Instantiate(child.gameObject, instance.transform);
#if UNITY_EDITOR
                }
#endif
                childInstance.transform.localPosition = child.localPosition;
                childInstance.transform.localRotation = child.localRotation;
                childInstance.transform.localScale = child.localScale;
            }
            
            #if UNITY_EDITOR
            if (isPrefab && instance.transform.childCount == 1)
            {
                var prefab = instance.transform.GetChild(0).gameObject;
                prefab.transform.SetParent(null, true);
                DestroyImmediate(instance);
                return prefab;
            }
            #endif
            
            return instance;
        }

        public void ApplyHideGroups(TileInstance instance, Transform root)
        {
            foreach (TileHideGroup group in hideGroups)
            {
                bool hidden = group.ShouldHide(instance.tilesetFlags);
                foreach (Transform child in root)
                {
                    if(group.GroupTag.Compare(child.gameObject)) child.gameObject.SetActive(!hidden);
                }
            }
        }
        
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            // Draw fancy tile gizmo...
            
            var m1 = Gizmos.matrix;
            var m2 = Handles.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Handles.matrix = transform.localToWorldMatrix;
            
            Gizmos.color = new Color(0, 0.5f, 1, 0.5f);
            Gizmos.DrawCube(new Vector3(0, 0, -Height / 2 - GIZMO_MARGIN / 2), new Vector3(Width + GIZMO_MARGIN * 2, 0, GIZMO_MARGIN));
            Gizmos.DrawCube(new Vector3(0, 0, Height / 2 + GIZMO_MARGIN / 2), new Vector3(Width + GIZMO_MARGIN * 2, 0, GIZMO_MARGIN));
            Gizmos.DrawCube(new Vector3(-Width / 2 - GIZMO_MARGIN / 2, 0, 0), new Vector3(GIZMO_MARGIN, 0, Height));
            Gizmos.DrawCube(new Vector3(Width / 2 + GIZMO_MARGIN / 2, 0, 0), new Vector3(GIZMO_MARGIN, 0, Height));

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(Width + GIZMO_MARGIN * 2, 0, Height + GIZMO_MARGIN * 2));
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(Width, 0, Height));

            Handles.color = Color.green;
            Handles.ArrowHandleCap(0, new Vector3(-Width / 2 - GIZMO_MARGIN / 2, 0, -Height / 2 - GIZMO_MARGIN / 2),
                Quaternion.LookRotation(Vector3.forward), GIZMO_MARGIN, EventType.Repaint);
            Handles.color = Color.red;
            Handles.ArrowHandleCap(0, new Vector3(-Width / 2 - GIZMO_MARGIN / 2, 0, -Height / 2 - GIZMO_MARGIN / 2),
                Quaternion.LookRotation(Vector3.right), GIZMO_MARGIN, EventType.Repaint);

            GUI.color = Color.white;
            var pos = new Vector3(0, 0, -Height / 2 - GIZMO_MARGIN * 1.25f);
            var size = Mathf.RoundToInt(HandleUtility.GetHandleSize(pos) + 13);
            Handles.Label(pos, Width.ToString(), new GUIStyle("Label"){fontSize = size});
            
            pos = new Vector3(-Width / 2 - GIZMO_MARGIN * 1.25f, 0, 0);
            size = Mathf.RoundToInt(HandleUtility.GetHandleSize(pos) + 13);
            Handles.Label(pos, Height.ToString(), new GUIStyle("Label"){fontSize = size});

            Gizmos.matrix = m1;
            Handles.matrix = m2;
#endif
        }

        public void SetBoundsFromChildren()
        {
            width = 0;
            height = 0;
            
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                var bounds = renderer.bounds.size;
                width = Mathf.Max(width, 
                    Mathf.Abs(Vector3.Dot(transform.right, renderer.transform.right * bounds.x)),
                    Mathf.Abs(Vector3.Dot(transform.right, renderer.transform.up * bounds.y)),
                    Mathf.Abs(Vector3.Dot(transform.right, renderer.transform.forward * bounds.z)));
                height = Mathf.Max(height, 
                    Mathf.Abs(Vector3.Dot(transform.up, renderer.transform.right * bounds.x)),
                    Mathf.Abs(Vector3.Dot(transform.up, renderer.transform.up * bounds.y)),
                    Mathf.Abs(Vector3.Dot(transform.up, renderer.transform.forward * bounds.z)));
            }
        }

        public void RotateWithoutChildren(Quaternion rotation)
        {
            transform.rotation = rotation * transform.rotation;
            var invRot = Quaternion.Inverse(rotation);

            foreach (Transform child in transform)
            {
                child.transform.rotation = invRot * child.transform.rotation;
            }
        }
    }
}
