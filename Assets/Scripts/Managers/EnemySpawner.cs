using JetBrains.Annotations;
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

    private const string giantTag2 = "Giant_lvl2";
    private const string vikingTag2 = "Viking_lvl2";
    private const string scoutTag2 = "Scout_lvl2";
    private const string wizardTag2 = "Wizard_lvl2";
    private const string archerTag2 = "Archer_lvl2";

    private const float intraBatchDelay = 0.2f;

    public void SpawnWave(WaveConfig config)
    {
        StartCoroutine(SpawnEnemyWaveCoroutine(config));
    }

    private IEnumerator SpawnEnemyWaveCoroutine(WaveConfig config)
    {
        if (config == null) yield break;

        int grandTotal =
            config.giantAmount + config.giantAmountLvl2 +
            config.vikingAmount + config.vikingAmountLvl2 +
            config.scoutAmount + config.scoutAmountLvl2 +
            config.wizardAmount + config.wizardAmountLvl2 +
            config.archerAmount + config.archerAmountLvl2;

        if (grandTotal == 0) yield break;

        bool firstGroup = true;

        IEnumerator ProcessUnitGroup(string tag1, int count1, string tag2, int count2)
        {
            int groupTotal = count1 + count2;

            if (groupTotal > 0)
            {
                if (!firstGroup)
                {
                    GameManager.Instance.MoveAllWaitingForward();
                    yield return new WaitForSeconds(spawnInterval);
                }

                firstGroup = false;

                yield return SpawnMixedLine(tag1, count1, tag2, count2, unitSpacing);
            }
        }

        yield return ProcessUnitGroup(giantTag, config.giantAmount, giantTag2, config.giantAmountLvl2);
        yield return ProcessUnitGroup(vikingTag, config.vikingAmount, vikingTag2, config.vikingAmountLvl2);
        yield return ProcessUnitGroup(scoutTag, config.scoutAmount, scoutTag2, config.scoutAmountLvl2);
        yield return ProcessUnitGroup(wizardTag, config.wizardAmount, wizardTag2, config.wizardAmountLvl2);
        yield return ProcessUnitGroup(archerTag, config.archerAmount, archerTag2, config.archerAmountLvl2);

        GameManager.Instance.SetState(GameState.Combat);
    }

    private IEnumerator SpawnMixedLine(string tag1, int count1, string tag2, int count2, float spacing)
    {
        int totalCount = count1 + count2;
        if (totalCount == 0) yield break;

        bool isGiant = (tag1 == giantTag || tag2 == giantTag2);
        int maxBatch = isGiant ? 1 : 3;

        int spawnedSoFar = 0;

        Vector3 center = enemyWaitPos.position;
        Vector3 designated = enemyGoDesignatedPointPos.position;

        while (spawnedSoFar < totalCount)
        {
            int remaining = totalCount - spawnedSoFar;
            int thisBatchSize = Mathf.Min(maxBatch, remaining);

            for (int j = 0; j < thisBatchSize; j++)
            {
                int currentIndex = spawnedSoFar + j;

                string currentTag = (currentIndex < count1) ? tag1 : tag2;

                Vector3 pos = enemySpawnPos.position;

                GameObject instance = ObjectPooler.Instance.SpawnFromPool(currentTag, pos, Quaternion.identity);

                if (instance != null)
                {
                    Person person = instance.GetComponent<Person>();
                    if (person != null)
                    {
                        GameManager.Instance.AddEnemy(person);
                        person.targetPosition = CalculateWaitPos(currentIndex, totalCount, center, spacing);
                        person.BeginDeployment(designated);
                    }
                }

                if (j < thisBatchSize - 1) yield return new WaitForSeconds(intraBatchDelay);
            }

            spawnedSoFar += thisBatchSize;
            if (spawnedSoFar < totalCount) yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Vector3 CalculateWaitPos(int index, int total, Vector3 center, float spacing)
    {
        float startX = -(total - 1f) * spacing / 2f;
        return center + new Vector3(startX + index * spacing, 0f, 0f);
    }
}