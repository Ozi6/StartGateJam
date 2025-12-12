using System.Collections.Generic;
using UnityEngine;

public class TeamTargetManager : MonoBehaviour
{
    public Dictionary<Person, int> playersTargetDictionary = new Dictionary<Person, int>();
    public Dictionary<Person, int> enemiesTargetDictionary = new Dictionary<Person, int>();
    [SerializeField] private bool playerSide;

    void Start()
    {
        InitializeTargetDictionary(playersTargetDictionary, true);
        InitializeTargetDictionary(enemiesTargetDictionary, false);
    }

    public void InitializeTargetDictionary(Dictionary<Person, int> dict, bool player)
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
