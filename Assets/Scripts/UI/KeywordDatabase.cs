using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KeywordDatabase", menuName = "ScriptableObjects/KeywordDatabase", order = 1)]
public class KeywordDatabase : ScriptableObject
{
    public List<string> validKeywords = new List<string>();
    public List<KeywordPair> pairs = new List<KeywordPair>();

    [System.Serializable]
    public class KeywordPair
    {
        public string first;
        public string second;
        public GameObject prefab;
    }

    private Dictionary<string, Dictionary<string, GameObject>> pairDict = null;

    private void BuildDictionary()
    {
        if (pairDict != null) return;
        pairDict = new Dictionary<string, Dictionary<string, GameObject>>();
        foreach (var pair in pairs)
        {
            string f = pair.first.ToLower();
            string s = pair.second.ToLower();
            if (!pairDict.ContainsKey(f))
            {
                pairDict[f] = new Dictionary<string, GameObject>();
            }
            pairDict[f][s] = pair.prefab;
        }
    }

    public bool IsValid(string word)
    {
        return validKeywords.Contains(word.ToLower());
    }

    public bool HasPair(string first, string second)
    {
        BuildDictionary();
        first = first.ToLower();
        second = second.ToLower();
        return pairDict.ContainsKey(first) && pairDict[first].ContainsKey(second);
    }

    public GameObject GetPrefab(string first, string second)
    {
        BuildDictionary();
        first = first.ToLower();
        second = second.ToLower();
        if (HasPair(first, second))
        {
            return pairDict[first][second];
        }
        return null;
    }
}