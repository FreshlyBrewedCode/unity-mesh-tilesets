using UnityEngine;

namespace MeshTilesets
{
    [System.Serializable]
    public class Tag
    {
        [SerializeField]
        private string tagName;
        public string TagName => tagName;

        public Tag(string tag)
        {
            tagName = tag;
        }

        public override string ToString()
        {
            return tagName;
        }

        public override bool Equals(object obj)
        {
            if (obj is Tag other) return string.Equals(tagName, other.TagName);
            if (obj is string s) return string.Equals(tagName, s);
            return base.Equals(obj);
        }

        public bool Compare(GameObject obj)
        {
            return obj.CompareTag(tagName);
        }
    }
}
