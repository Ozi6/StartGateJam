using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Transforms")]
    public Transform enemySpawnPos;
    public Transform enemyGoDesignatedPointPos;
    public Transform enemyWaitPos;

    [Header("Spawn Settings")]
    public float spawnInterval = .25f;
    public float unitSpacing = 1.5f;

    private const string giantTag = "Giant";
    private const string vikingTag = "Viking";
    private const string scoutTag = "Scout";
    private const string wizardTag = "Wizard";
    private const string archerTag = "Archer";
    private const float intraBatchDelay = 0.2f;

    public void SpawnWave(WaveConfig config)
    {
        StartCoroutine(SpawnEnemyWaveCoroutine(config));
    }

    private IEnumerator SpawnEnemyWaveCoroutine(WaveConfig config)
    {
        if (config == null) yield break;

        int total = config.giantAmount + config.vikingAmount + config.scoutAmount +
                    config.wizardAmount + config.archerAmount;
        if (total == 0) yield break;

        bool first = true;

        IEnumerator SpawnIfNotEmpty(string tag, int amount)
        {
            if (amount > 0)
            {
                if (!first)
                {
                    GameManager.Instance.MoveAllWaitingForward();
                    yield return new WaitForSeconds(spawnInterval);
                }
                first = false;
                yield return SpawnUnitTypeInLine(tag, amount, unitSpacing);
            }
        }

        yield return SpawnIfNotEmpty(giantTag, config.giantAmount);
        yield return SpawnIfNotEmpty(vikingTag, config.vikingAmount);
        yield return SpawnIfNotEmpty(scoutTag, config.scoutAmount);
        yield return SpawnIfNotEmpty(wizardTag, config.wizardAmount);
        yield return SpawnIfNotEmpty(archerTag, config.archerAmount);
    }

    private IEnumerator SpawnUnitTypeInLine(string tag, int count, float spacing)
    {
        if (count == 0 || string.IsNullOrEmpty(tag)) yield break;

        int maxBatch = tag == giantTag ? 1 : 3;
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
                    GameManager.Instance.AddEnemy(person);
                    person.targetPosition = CalculateWaitPos(index, count, center, spacing);
                    person.BeginDeployment(designated);
                }

                if (j < thisBatch - 1) yield return new WaitForSeconds(intraBatchDelay);
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
}