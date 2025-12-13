using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Augments/Augment Icon Database")]
public class AugmentIconDatabase : ScriptableObject
{
    [System.Serializable]
    public struct AugmentIconEntry
    {
        public int augmentId;
        public Sprite icon;
    }

    [SerializeField] private List<AugmentIconEntry> icons = new();

    private Dictionary<int, Sprite> iconById;

    private void OnEnable()
    {
        iconById = new Dictionary<int, Sprite>();
        foreach (var entry in icons)
        {
            if (!iconById.ContainsKey(entry.augmentId))
                iconById.Add(entry.augmentId, entry.icon);
        }
    }

    public Sprite GetIcon(int augmentId)
    {
        if (iconById != null && iconById.TryGetValue(augmentId, out var icon))
            return icon;

        return null;
    }
}
