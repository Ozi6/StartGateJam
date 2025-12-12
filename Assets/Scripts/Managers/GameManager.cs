using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<Person> playersTeam = new List<Person>();
    public List<Person> enemyTeam = new List<Person>();

    public GameObject currentThrowableHeld;

    protected override void OnAwake()
    {
        playersTeam = new List<Person>();
        enemyTeam = new List<Person>();
    }
}