using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<Person> playersTeam = new List<Person>();
    public List<Person> enemyTeam = new List<Person>();
    public GameObject currentThrowableHeld;

    [Header("Manager References")]
    public EnemySpawner enemySpawner;
    public InputManager inputManager;
    public UIManager uiManager;
    public KeywordDatabase database;

    [Header("General Settings")]
    public WaveConfig testWaveConfig;
    public float rowSpacing = 3f;

    protected override void OnAwake()
    {
        playersTeam = new List<Person>();
        enemyTeam = new List<Person>();

        if (enemySpawner == null) enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (inputManager == null) inputManager = FindFirstObjectByType<InputManager>();
        if (uiManager == null) uiManager = FindFirstObjectByType<UIManager>();

        if (uiManager != null)
        {
            uiManager.InitializeUI();
        }

        if (enemySpawner != null && testWaveConfig != null)
        {
            enemySpawner.SpawnWave(testWaveConfig);
        }

        if (inputManager != null)
        {
            inputManager.Initialize(uiManager, database, this);
        }
    }

    public void AddEnemy(Person newEnemy)
    {
        enemyTeam.Add(newEnemy);
    }

    public void MoveAllWaitingForward()
    {
        Vector3 shift = Vector3.back * rowSpacing;
        foreach (Person person in enemyTeam)
        {
            person.targetPosition += shift;
        }
    }
}