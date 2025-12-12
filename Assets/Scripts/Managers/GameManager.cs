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

    protected override void OnAwake()
    {
        playersTeam = new List<Person>();
        enemyTeam = new List<Person>();
    }

    public void SpawnEnemyWave(WaveConfig config)
    {
        if (config == null) return;

        int total = config.giantAmount + config.vikingAmount + config.scoutAmount + config.wizardAmount + config.archerAmount;
        if (total == 0) return;

        List<Person> spawned = new List<Person>();
        float spacing = 2f;
        float startX = -(total - 1) * spacing / 2f;
        Vector3 basePos = enemySpawnPos.position;

        int index = 0;
        SpawnMultiple(giantTag, config.giantAmount, ref index, startX, spacing, basePos, spawned);
        SpawnMultiple(vikingTag, config.vikingAmount, ref index, startX, spacing, basePos, spawned);
        SpawnMultiple(scoutTag, config.scoutAmount, ref index, startX, spacing, basePos, spawned);
        SpawnMultiple(wizardTag, config.wizardAmount, ref index, startX, spacing, basePos, spawned);
        SpawnMultiple(archerTag, config.archerAmount, ref index, startX, spacing, basePos, spawned);

        enemyTeam.AddRange(spawned);
    }

    private void SpawnMultiple(string tag, int count, ref int index, float startX, float spacing, Vector3 basePos, List<Person> spawnedList)
    {
        if (count == 0 || string.IsNullOrEmpty(tag)) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = basePos + new Vector3(startX + index * spacing, 0, 0);
            GameObject instance = ObjectPooler.Instance.SpawnFromPool(tag, pos, Quaternion.identity);
            if (instance == null) continue;
            Person person = instance.GetComponent<Person>();
            if (person != null)
            {
                spawnedList.Add(person);
            }
            index++;
        }
    }
}