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
    public WaveConfig testWaveConfig;

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;

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

        float spacing = 2f;
        Vector3 basePos = enemySpawnPos.position;
        if (config.giantAmount > 0)
        {
            SpawnUnitTypeInLine(giantTag, config.giantAmount, spacing, basePos);
            yield return new WaitForSeconds(spawnInterval);
        }
        if (config.vikingAmount > 0)
        {
            SpawnUnitTypeInLine(vikingTag, config.vikingAmount, spacing, basePos);
            yield return new WaitForSeconds(spawnInterval);
        }
        if (config.scoutAmount > 0)
        {
            SpawnUnitTypeInLine(scoutTag, config.scoutAmount, spacing, basePos);
            yield return new WaitForSeconds(spawnInterval);
        }
        if (config.wizardAmount > 0)
        {
            SpawnUnitTypeInLine(wizardTag, config.wizardAmount, spacing, basePos);
            yield return new WaitForSeconds(spawnInterval);
        }
        if (config.archerAmount > 0)
        {
            SpawnUnitTypeInLine(archerTag, config.archerAmount, spacing, basePos);
        }
    }

    private void SpawnUnitTypeInLine(string tag, int count, float spacing, Vector3 basePos)
    {
        if (count == 0 || string.IsNullOrEmpty(tag)) return;
        float startX = -(count - 1) * spacing / 2f;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = basePos + new Vector3(startX + i * spacing, 0, 0);
            GameObject instance = ObjectPooler.Instance.SpawnFromPool(tag, pos, Quaternion.identity);
            if (instance == null) continue;
            Person person = instance.GetComponent<Person>();
            if (person != null)
                enemyTeam.Add(person);
        }
    }
}