using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; }
    public List<Person> playersTeam = new List<Person>();
    public List<Person> enemyTeam = new List<Person>();
    public int currentGold = 100;
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
        //playersTeam = new List<Person>();
        enemyTeam = new List<Person>();
        if (enemySpawner == null) enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (inputManager == null) inputManager = FindFirstObjectByType<InputManager>();
        if (uiManager == null) uiManager = FindFirstObjectByType<UIManager>();
        uiManager?.InitializeUI();
        inputManager?.Initialize(uiManager, database, this);
        SetState(GameState.Shopping);
    }
    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;
        ExitState(CurrentState);
        CurrentState = newState;
        EnterState(CurrentState);
    }
    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.Shopping:
                uiManager.ShowShopUI(true); 
                break;
            case GameState.Deployment:
                uiManager.ShowShopUI(false);
                if (enemySpawner != null && testWaveConfig != null)
                    enemySpawner.SpawnWave(testWaveConfig);
                break;
            case GameState.Combat:
                TeamTargetManager.Instance.AssignTargets();
                //inputManager.EnableInput(true);
                break;
            case GameState.Upgrade:
                //uiManager.ShowUpgradeUI(true);
                //inputManager.EnableInput(false);
                break;
        }
    }
    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.Shopping:
                //uiManager.ShowShopUI(false);
                break;
            case GameState.Deployment:
                break;
            case GameState.Combat:
                break;
            case GameState.Upgrade:
                //uiManager.ShowUpgradeUI(false);
                break;
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