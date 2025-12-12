using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<Person> playersTeam = new List<Person>();
    public List<Person> enemyTeam = new List<Person>();
    public GameObject currentThrowableHeld;
    private const string giantTag = "Giant";
    private const string vikingTag = "Viking";
    private const string scoutTag = "Scout";
    private const string wizardTag = "Wizard";
    private const string archerTag = "Archer";
    public Transform enemySpawnPos;
    public Transform enemyGoDesignatedPointPos;
    public Transform enemyWaitPos;
    public WaveConfig testWaveConfig;
    [Header("Spawn Settings")]
    public float spawnInterval = .25f;
    public float rowSpacing = 3f;
    [Header("UI Settings")]
    public GameObject speechBubblePrefab;
    public Canvas canvas;
    public GameObject sidePanel;
    [Header("Keyword UI")]
    public TMP_Text firstKeywordText;
    public TMP_Text secondKeywordText;
    public KeywordDatabase database;
    private GameObject speechBubbleInstance;
    private TMP_Text bubbleText;
    private string currentBubbleText = "";
    private string heldKeyword = "";
    private bool panelOpen = false;
    private Vector2 panelClosedPos;
    private Vector2 panelOpenPos;

    protected override void OnAwake()
    {
        playersTeam = new List<Person>();
        enemyTeam = new List<Person>();
        StartCoroutine(SpawnEnemyWaveCoroutine(testWaveConfig));
        if (sidePanel != null)
        {
            RectTransform rt = sidePanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                panelOpenPos = rt.anchoredPosition;
                panelClosedPos = new Vector2(-rt.rect.width, panelOpenPos.y);
                rt.anchoredPosition = panelClosedPos;
            }
        }
        if (firstKeywordText != null)
        {
            firstKeywordText.gameObject.SetActive(false);
        }
        if (secondKeywordText != null)
        {
            secondKeywordText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        foreach (char c in Input.inputString)
        {
            if (char.IsLetter(c))
            {
                if (currentThrowableHeld != null) continue;
                if (speechBubbleInstance == null)
                {
                    speechBubbleInstance = Instantiate(speechBubblePrefab, canvas.transform);
                    bubbleText = speechBubbleInstance.GetComponentInChildren<TMP_Text>();
                }
                speechBubbleInstance.SetActive(true);
                currentBubbleText += char.ToLower(c);
                if (bubbleText != null)
                    bubbleText.text = currentBubbleText;
                if (database.IsValid(currentBubbleText))
                {
                    string matched = currentBubbleText;
                    currentBubbleText = "";
                    if (bubbleText != null)
                        bubbleText.text = currentBubbleText;
                    speechBubbleInstance.SetActive(false);
                    if (heldKeyword == "")
                    {
                        heldKeyword = matched;
                        if (firstKeywordText != null)
                        {
                            firstKeywordText.text = matched;
                            firstKeywordText.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (database.HasPair(heldKeyword, matched))
                        {
                            GameObject prefab = database.GetPrefab(heldKeyword, matched);
                            if (prefab != null)
                            {
                                currentThrowableHeld = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                            }
                            if (secondKeywordText != null)
                            {
                                secondKeywordText.text = matched;
                                secondKeywordText.gameObject.SetActive(true);
                                StartCoroutine(HideSecondUI(0.5f));
                            }
                            heldKeyword = "";
                        }
                        else
                        {
                            heldKeyword = matched;
                            if (firstKeywordText != null)
                            {
                                firstKeywordText.text = matched;
                            }
                        }
                    }
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (!string.IsNullOrEmpty(currentBubbleText))
            {
                currentBubbleText = "";
                if (bubbleText != null)
                {
                    bubbleText.text = currentBubbleText;
                }
                if (speechBubbleInstance != null)
                {
                    speechBubbleInstance.SetActive(false);
                }
            }
            else if (heldKeyword != "")
            {
                heldKeyword = "";
                if (firstKeywordText != null)
                {
                    firstKeywordText.text = "";
                    firstKeywordText.gameObject.SetActive(false);
                }
            }
            else if (currentThrowableHeld != null)
            {
                Destroy(currentThrowableHeld);
                currentThrowableHeld = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            panelOpen = !panelOpen;
            StartCoroutine(SlidePanel(panelOpen));
        }
    }

    private IEnumerator HideSecondUI(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (secondKeywordText != null)
        {
            secondKeywordText.text = "";
            secondKeywordText.gameObject.SetActive(false);
            firstKeywordText.text = "";
            firstKeywordText.gameObject.SetActive(false);
        }
    }

    private IEnumerator SlidePanel(bool open)
    {
        if (sidePanel == null) yield break;
        RectTransform rt = sidePanel.GetComponent<RectTransform>();
        if (rt == null) yield break;
        Vector2 start = rt.anchoredPosition;
        Vector2 end = open ? panelOpenPos : panelClosedPos;
        float time = 0f;
        float duration = 0.5f;
        while (time < duration)
        {
            time += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(start, end, time / duration);
            yield return null;
        }
        rt.anchoredPosition = end;
    }

    public void SpawnEnemyWave(WaveConfig config)
    {
        StartCoroutine(SpawnEnemyWaveCoroutine(config));
    }

    private IEnumerator SpawnEnemyWaveCoroutine(WaveConfig config)
    {
        if (config == null) yield break;
        int total = config.giantAmount + config.vikingAmount + config.scoutAmount +
                    config.wizardAmount + config.archerAmount;
        if (total == 0) yield break;
        float spacing = 1.5f;
        bool first = true;
        if (config.giantAmount > 0)
        {
            if (!first)
            {
                MoveAllWaitingForward();
                yield return new WaitForSeconds(spawnInterval);
            }
            first = false;
            yield return SpawnUnitTypeInLine(giantTag, config.giantAmount, spacing);
        }
        if (config.vikingAmount > 0)
        {
            if (!first)
            {
                MoveAllWaitingForward();
                yield return new WaitForSeconds(spawnInterval);
            }
            first = false;
            yield return SpawnUnitTypeInLine(vikingTag, config.vikingAmount, spacing);
        }
        if (config.scoutAmount > 0)
        {
            if (!first)
            {
                MoveAllWaitingForward();
                yield return new WaitForSeconds(spawnInterval);
            }
            first = false;
            yield return SpawnUnitTypeInLine(scoutTag, config.scoutAmount, spacing);
        }
        if (config.wizardAmount > 0)
        {
            if (!first)
            {
                MoveAllWaitingForward();
                yield return new WaitForSeconds(spawnInterval);
            }
            first = false;
            yield return SpawnUnitTypeInLine(wizardTag, config.wizardAmount, spacing);
        }
        if (config.archerAmount > 0)
        {
            if (!first)
            {
                MoveAllWaitingForward();
                yield return new WaitForSeconds(spawnInterval);
            }
            first = false;
            yield return SpawnUnitTypeInLine(archerTag, config.archerAmount, spacing);
        }
    }

    private IEnumerator SpawnUnitTypeInLine(string tag, int count, float spacing)
    {
        if (count == 0 || string.IsNullOrEmpty(tag)) yield break;
        int maxBatch = tag == giantTag ? 1 : 3;
        float intraDelay = 0.2f;
        int spawned = 0;
        Vector3 center = enemyWaitPos.position;
        Vector3 designated = enemyGoDesignatedPointPos.position;
        while (spawned < count)
        {
            int thisBatch = Mathf.Min(maxBatch, count - spawned);
            for (int j = 0; j < thisBatch; j++)
            {
                int index = spawned + j;
                Vector3 pos = enemySpawnPos.position;
                GameObject instance = ObjectPooler.Instance.SpawnFromPool(tag, pos, Quaternion.identity);
                if (instance == null) continue;
                Person person = instance.GetComponent<Person>();
                if (person != null)
                {
                    enemyTeam.Add(person);
                    person.targetPosition = CalculateWaitPos(index, count, center, spacing);
                    person.BeginDeployment(designated);
                }
                if (j < thisBatch - 1) yield return new WaitForSeconds(intraDelay);
            }
            spawned += thisBatch;
            if (spawned < count) yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Vector3 CalculateWaitPos(int index, int total, Vector3 center, float spacing)
    {
        float startX = -(total - 1f) * spacing / 2f;
        return center + new Vector3(startX + index * spacing, 0f, 0f);
    }

    private void MoveAllWaitingForward()
    {
        Vector3 shift = Vector3.back * rowSpacing;
        foreach (Person person in enemyTeam)
        {
            person.targetPosition += shift;
        }
    }
}