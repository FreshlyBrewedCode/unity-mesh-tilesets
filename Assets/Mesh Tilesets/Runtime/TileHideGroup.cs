using UnityEngine;

namespace MeshTilesets
{
    [System.Serializable]
    public class TileHideGroup
    {
        public enum HideMode
        {
            ShowOnMatch,
            HideOnMatch
        }
        
        [SerializeField] private Tag groupTag;
        [SerializeField] private HideMode hideMode;
        [SerializeField] private TilesetFlagsMask tilesetFlags;

        public Tag GroupTag => groupTag;
        
        public bool ShouldHide(TilesetFlagsMask flags)
        {
            return hideMode == HideMode.HideOnMatch ? flags.Matches(tilesetFlags) : !flags.Matches(tilesetFlags);
        }
    }
}
