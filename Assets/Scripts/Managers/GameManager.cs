using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    [SerializeField] private List<WaveConfig> waveConfigs = new List<WaveConfig>();
    [SerializeField] private TMP_Text unitCountText;
    public WaveConfig CurrentWaveConfig { get; private set; }
    private int currentWaveIndex = 0;
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

        if (waveConfigs.Count > 0)
        {
            CurrentWaveConfig = waveConfigs[0];
        }

        SetState(GameState.Shopping);
        UpdateUnitCountDisplay();
    }

    void Update()
    {
        if (CurrentState == GameState.Combat)
        {
            if (enemyTeam.Count == 0)
            {
                // Combat ends, proceed to next wave
                currentWaveIndex++;
                if (currentWaveIndex < waveConfigs.Count)
                {
                    CurrentWaveConfig = waveConfigs[currentWaveIndex];
                    SetState(GameState.Shopping);
                    UpdateUnitCountDisplay();
                }
                else
                {
                    // All waves completed, handle win condition
                    Debug.Log("All waves completed! You win!");
                    // Optionally set to a Win state or something
                }
            }
            else if (playersTeam.Count == 0)
            {
                // Handle lose condition
                Debug.Log("Player team defeated! Game over.");
                // Optionally set to a Lose state
            }
        }
    }

    public void SetState(GameState newState)
    {
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
                UpdateUnitCountDisplay();
                break;
            case GameState.Deployment:
                uiManager.ShowShopUI(false);
                if (enemySpawner != null && CurrentWaveConfig != null)
                    enemySpawner.SpawnWave(CurrentWaveConfig);
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

    public void UpdateUnitCountDisplay()
    {
        if (unitCountText != null && CurrentWaveConfig != null)
        {
            unitCountText.text = $"{playersTeam.Count}/{CurrentWaveConfig.maxPlaceableUnits}";
        }
    }
}