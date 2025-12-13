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

    // Team snapshot storage
    private List<PersonSnapshot> teamSnapshot = new List<PersonSnapshot>();

    protected override void OnAwake()
    {
        playersTeam = new List<Person>();
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
                // Combat ends, restore team from snapshot
                RestoreTeamFromSnapshot();

                // Proceed to next wave
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
            if (enemyTeam.Count == 0)
            {
                RestoreTeamFromSnapshot();

                // Move to augmentation instead of shopping
                SetState(GameState.Augmentation);
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
                inputManager.typerEnable = false;
                uiManager.ShowShopUI(true);
                UpdateUnitCountDisplay();
                break;
            case GameState.Deployment:
                uiManager.ShowShopUI(false);
                if (enemySpawner != null && CurrentWaveConfig != null)
                    enemySpawner.SpawnWave(CurrentWaveConfig);
                break;
            case GameState.Combat:
                inputManager.typerEnable = true;
                TakeTeamSnapshot();
                TeamTargetManager.Instance.AssignTargets();
                //inputManager.EnableInput(true);
                break;
            case GameState.Augmentation:
                inputManager.typerEnable = false;
                uiManager.ShowShopUI(false);
                uiManager.ShowAugmentSelection();
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
            case GameState.Augmentation:
                uiManager.HideAugmentSelection();
                break;
        }
    }

    private void TakeTeamSnapshot()
    {
        teamSnapshot.Clear();

        foreach (Person person in playersTeam)
        {
            PersonSnapshot snapshot = new PersonSnapshot
            {
                prefabName = person.gameObject.name.Replace("(Clone)", "").Trim(),
                position = person.transform.position,
                rotation = person.transform.rotation,
                targetPosition = person.targetPosition,
                currentHealth = person.maxHealth,
                maxHealth = person.maxHealth
            };

            teamSnapshot.Add(snapshot);
        }

        Debug.Log($"Team snapshot taken: {teamSnapshot.Count} units");
    }

    private void RestoreTeamFromSnapshot()
    {
        // Clear current team and deactivate/destroy current units
        foreach (Person person in playersTeam)
        {
            if (person != null && person.gameObject != null)
            {
                // Try to return to pool if using object pooling
                IPooledObject pooledObj = person.GetComponent<IPooledObject>();
                if (pooledObj != null)
                {
                    // Determine the pool tag (you may need to adjust this based on your setup)
                    string poolTag = person.gameObject.name.Replace("(Clone)", "").Trim();
                    ObjectPooler.Instance?.ReturnToPool(person.gameObject, poolTag);
                }
                else
                {
                    Destroy(person.gameObject);
                }
            }
        }
        playersTeam.Clear();

        // Restore team from snapshot
        foreach (PersonSnapshot snapshot in teamSnapshot)
        {
            GameObject restoredUnit = null;

            // Try to spawn from pool first
            if (ObjectPooler.Instance != null)
            {
                restoredUnit = ObjectPooler.Instance.SpawnFromPool(
                    snapshot.prefabName,
                    snapshot.position,
                    snapshot.rotation
                );
            }

            if (restoredUnit != null)
            {
                Person person = restoredUnit.GetComponent<Person>();
                if (person != null)
                {
                    person.targetPosition = snapshot.targetPosition;
                    person.health = person.maxHealth;
                    person.SetFriendly(true); // Ensure health bar color is correct
                    person.OnObjectSpawn(); // This will register the unit via UnitRegistrar (prevents duplicates)
                }
            }
            else
            {
                Debug.LogWarning($"Failed to restore unit: {snapshot.prefabName}");
            }
        }

        Debug.Log($"Team restored from snapshot: {playersTeam.Count} units");
        UpdateUnitCountDisplay();
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
        // Clean up null references from the list
        CleanupNullUnits();
        
        if (unitCountText != null && CurrentWaveConfig != null)
        {
            unitCountText.text = $"{playersTeam.Count}/{CurrentWaveConfig.maxPlaceableUnits}";
        }
    }

    /// <summary>
    /// Removes null or destroyed units from the playersTeam list to ensure accurate counting.
    /// </summary>
    private void CleanupNullUnits()
    {
        playersTeam.RemoveAll(person => person == null || person.gameObject == null || !person.gameObject.activeSelf);
    }
}

// Snapshot class to store unit data
[System.Serializable]
public class PersonSnapshot
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 targetPosition;
    public int currentHealth;
    public int maxHealth;
    // Add any other properties you need to restore
}