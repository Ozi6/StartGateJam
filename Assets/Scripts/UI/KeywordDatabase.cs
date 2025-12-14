using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "KeywordDatabase",
    menuName = "ScriptableObjects/KeywordDatabase",
    order = 1)]
public class KeywordDatabase : ScriptableObject
{
    [Header("Valid single keywords")]
    public List<string> validKeywords = new List<string>();

    [Header("Keyword pairs (order independent)")]
    public List<KeywordPair> pairs = new List<KeywordPair>();

    [System.Serializable]
    public class KeywordPair
    {
        public string first;
        public string second;
        public GameObject prefab;
        public PowerUpType powerUpType;
    }

    // ---------- Runtime dictionaries ----------
    private Dictionary<string, Dictionary<string, GameObject>> prefabDict;
    private Dictionary<(string, string), PowerUpType> powerUpDict;

    // ---------- Build ----------
    private void BuildDictionaries()
    {
        if (prefabDict != null) return;

        prefabDict = new Dictionary<string, Dictionary<string, GameObject>>();
        powerUpDict = new Dictionary<(string, string), PowerUpType>();

        foreach (var pair in pairs)
        {
            string a = pair.first.ToLower();
            string b = pair.second.ToLower();

            AddPrefabPair(a, b, pair.prefab);
            AddPrefabPair(b, a, pair.prefab);

            powerUpDict[(a, b)] = pair.powerUpType;
            powerUpDict[(b, a)] = pair.powerUpType;
        }
    }

    private void AddPrefabPair(string first, string second, GameObject prefab)
    {
        if (!prefabDict.ContainsKey(first))
            prefabDict[first] = new Dictionary<string, GameObject>();

        prefabDict[first][second] = prefab;
    }

    // ---------- Public API ----------

    public bool IsValid(string word)
    {
        return validKeywords.Contains(word.ToLower());
    }

    public bool HasPair(string first, string second)
    {
        BuildDictionaries();

        first = first.ToLower();
        second = second.ToLower();

        return prefabDict.ContainsKey(first)
            && prefabDict[first].ContainsKey(second);
    }

    public GameObject GetPrefab(string first, string second)
    {
        BuildDictionaries();

        first = first.ToLower();
        second = second.ToLower();

        if (HasPair(first, second))
            return prefabDict[first][second];

        return null;
    }

    public PowerUpType GetPowerUp(string first, string second)
    {
        BuildDictionaries();

        first = first.ToLower();
        second = second.ToLower();

        return powerUpDict[(first, second)];
    }

    public GameObject GetPrefabByPowerUpType(PowerUpType type)
    {
        foreach (var pair in pairs)
        {
            if (pair.powerUpType == type) return pair.prefab;
        }
        return null;
    }
}