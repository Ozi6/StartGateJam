using System.Collections;
using System.Collections.Generic;
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

    protected override void OnAwake()
    {
        playersTeam = new List<Person>();
        enemyTeam = new List<Person>();
        StartCoroutine(SpawnEnemyWaveCoroutine(testWaveConfig));
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