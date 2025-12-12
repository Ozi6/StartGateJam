using System.Collections.Generic;
using UnityEngine;

public class TeamTargetManager : MonoBehaviour
{
    public Dictionary<Person, float> playersTargetDictionary = new Dictionary<Person, float>();
    public Dictionary<Person, float> enemiesTargetDictionary = new Dictionary<Person, float>();
    [SerializeField] private bool playerSide;

    void Start()
    {
        InitializeTargetDictionary(playersTargetDictionary, true);
        InitializeTargetDictionary(enemiesTargetDictionary, false);
    }

    public void InitializeTargetDictionary(Dictionary<Person, float> dict, bool player)
    {
        dict.Clear();
        List<Person> enemies = player ? GameManager.Instance.enemyTeam : GameManager.Instance.playersTeam;
        foreach (Person enemy in enemies)
        {
            if (enemy != null && !dict.ContainsKey(enemy))
                dict.Add(enemy, 0);
        }
    }
}
