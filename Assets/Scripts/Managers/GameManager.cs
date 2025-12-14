using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UIElements;

public class GameManager : Singleton<GameManager>
{
    protected override bool Persistent => false;
    public GameState CurrentState { get; private set; }
    public List<Person> playersTeam = new List<Person>();
    public List<Person> enemyTeam = new List<Person>();
    public int currentGold = 100;
    public GameObject currentThrowableHeld;
    [SerializeField] public TMP_Text gold;
    [Header("Manager References")]
    public EnemySpawner enemySpawner;
    public InputManager inputManager;
    public UIManager uiManager;
    public KeywordDatabase database;

    [Header("General Settings")]
    [SerializeField] private List<WaveConfig> waveConfigs = new List<WaveConfig>();
    [SerializeField] private TMP_Text unitCountText;
    [SerializeField] private TMP_Text levelText;
    public WaveConfig CurrentWaveConfig { get; private set; }
    private int currentWaveIndex = 0;
    public float rowSpacing = 3f;

    // Team snapshot storage
    private List<PersonSnapshot> teamSnapshot = new List<PersonSnapshot>();
    [SerializeField] public Animator playerAnimator;

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
        gold.text = currentGold.ToString();
        if (CurrentState != GameState.Combat || isEnding)
            return;

        CleanupNullUnits();
        CleanupNullEnemies();

        if (enemyTeam.Count == 0)
        {
            RestoreTeamFromSnapshot();
            currentWaveIndex++;

            if (currentWaveIndex < waveConfigs.Count)
            {
                CurrentWaveConfig = waveConfigs[currentWaveIndex];
                Debug.Log("Right now it is " + currentWaveIndex);
                SetState(GameState.Augmentation);
                UpdateUnitCountDisplay();
            }
            else
            {
                Debug.Log("All waves completed! You win!");
                StartCoroutine(EndGameAfterAnimation(GameResult.Win));
            }
        }
        else if (playersTeam.Count == 0)
        {
            Debug.Log("Game Over! You lose!");
            StartCoroutine(EndGameAfterAnimation(GameResult.Lose));
        }

        
    }


    private bool isEnding = false;

    private IEnumerator EndGameAfterAnimation(GameResult result)
    {
        if (isEnding) yield break;
        isEnding = true;

        GameResultHolder.Result = result;

        if (result == GameResult.Win)
            playerAnimator.SetTrigger("GameWin");
        else
            playerAnimator.SetTrigger("GameOver");

        yield return new WaitForSeconds(3.5f);

        SceneManager.LoadScene("EndScreen");
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
                inputManager.goldPowerSummonedCount = 0;
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
                AudioManager.Instance.PlaySFX("alkis_ambians");
                //inputManager.EnableInput(true);
                break;
            case GameState.Augmentation:
                inputManager.Reset();
                uiManager.ResetSpells();
                uiManager.UpdateSpeechBubble("");
                inputManager.typerEnable = false;
                IncrementLevel();
                uiManager.ShowShopUI(false);
                uiManager.ShowAugmentSelection();
                break;

        }
    }

    private void IncrementLevel()
    {
        string text = levelText.text;

        int lastSpace = text.LastIndexOf(' ');
        if (lastSpace == -1) return;

        string prefix = text.Substring(0, lastSpace + 1);
        string roman = text.Substring(lastSpace + 1);

        int value = RomanToInt(roman);
        value++;

        levelText.text = prefix + IntToRoman(value);
    }

    private int RomanToInt(string roman)
    {
        int total = 0;
        int prev = 0;

        foreach (char c in roman)
        {
            int value = c switch
            {
                'I' => 1,
                'V' => 5,
                'X' => 10,
                'L' => 50,
                'C' => 100,
                'D' => 500,
                _ => 0
            };

            total += value > prev ? value - 2 * prev : value;
            prev = value;
        }

        return total;
    }

    private string IntToRoman(int number)
    {
        (int, string)[] map =
        {
        (500,"D"), (400,"CD"),
        (100,"C"), (90,"XC"), (50,"L"), (40,"XL"),
        (10,"X"), (9,"IX"), (5,"V"), (4,"IV"), (1,"I")
    };

        string result = "";
        foreach (var (value, symbol) in map)
        {
            while (number >= value)
            {
                result += symbol;
                number -= value;
            }
        }
        return result;
    }

    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.Shopping:
                tagAllFriendlyUnits();
                //uiManager.ShowShopUI(false);
                break;
            case GameState.Deployment:
                break;
            case GameState.Combat:
                AudioManager.Instance.PlaySFX("alkis_ambians");
                currentThrowableHeld = null;
                break;
            case GameState.Augmentation:
                uiManager.HideAugmentSelection();
                break;
        }
    }

    private void tagAllFriendlyUnits()
    {
        foreach ( Person p in playersTeam)
        {
            p.isNew = false;
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
        Vector3 shift = Vector3.left * rowSpacing;
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
    private void CleanupNullEnemies()
    {
        enemyTeam.RemoveAll(person => person == null || person.gameObject == null || !person.gameObject.activeSelf);
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