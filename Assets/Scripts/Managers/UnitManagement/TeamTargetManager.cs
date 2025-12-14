using System.Collections.Generic;
using UnityEngine;

public class TeamTargetManager : Singleton<TeamTargetManager>
{
    public Dictionary<Person, float> playersTargetDictionary = new Dictionary<Person, float>();
    public Dictionary<Person, float> enemiesTargetDictionary = new Dictionary<Person, float>();
    protected override bool Persistent => false;
    void Start()
    {
        // Initialization moved to AssignTargets to ensure teams are populated
    }
    public void InitializeTargetDictionary(Dictionary<Person, float> dict, bool player)
    {
        dict.Clear();
        List<Person> targets = player ? GameManager.Instance.enemyTeam : GameManager.Instance.playersTeam;
        foreach (Person target in targets)
        {
            if (target != null && target.gameObject.activeSelf && !dict.ContainsKey(target))
                dict.Add(target, 0);
        }
    }
    public void AssignTargets()
    {
        InitializeTargetDictionary(playersTargetDictionary, true);
        InitializeTargetDictionary(enemiesTargetDictionary, false);
        foreach (Person person in GameManager.Instance.playersTeam)
        {
            if (person != null && person.gameObject.activeSelf)
            {
                Person target = FindBestTarget(person, playersTargetDictionary);
                if (target != null)
                {
                    person.TargetEntity = target;
                    playersTargetDictionary[target] += 1;
                }
            }
        }
        foreach (Person person in GameManager.Instance.enemyTeam)
        {
            if (person != null && person.gameObject.activeSelf)
            {
                Person target = FindBestTarget(person, enemiesTargetDictionary);
                if (target != null)
                {
                    person.TargetEntity = target;
                    enemiesTargetDictionary[target] += 1;
                }
            }
        }
    }

    private const float LOAD_WEIGHT = 1000f;       // Dominates target assignment
    private const float DISTANCE_WEIGHT = 1.5f;    // Distance preference multiplier

    private Person FindBestTarget(Person selector, Dictionary<Person, float> targetsDict)
    {
        if (targetsDict.Count == 0) return null;

        Person bestTarget = null;
        float bestScore = float.MaxValue;

        foreach (var kvp in targetsDict)
        {
            Person target = kvp.Key;
            if (target == null || !target.gameObject.activeSelf)
                continue;

            float load = kvp.Value;
            float dist = Vector3.Distance(selector.transform.position, target.transform.position);

            float score = load * LOAD_WEIGHT + dist * DISTANCE_WEIGHT;

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = target;
            }
        }

        return bestTarget;
    }

    public Person GetNewTarget(Person selector)
    {
        Dictionary<Person, float> targetsDict = selector.IsFriendly ? playersTargetDictionary : enemiesTargetDictionary;
        List<Person> toRemove = new List<Person>();
        foreach (Person target in targetsDict.Keys)
        {
            if (target == null || !target.gameObject.activeSelf)
                toRemove.Add(target);
        }
        foreach (Person remove in toRemove)
        {
            targetsDict.Remove(remove);
        }
        Person bestTarget = FindBestTarget(selector, targetsDict);
        if (bestTarget != null)
        {
            targetsDict[bestTarget] += 1;
        }
        return bestTarget;
    }
}