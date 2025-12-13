using System.Collections.Generic;
using UnityEngine;

public class AugmentHandler : Singleton<AugmentHandler>
{
    [System.Serializable]
    public class Augment
    {
        public int id;
        public string title;
        public string description;
        public float chance;
        public bool repeatable;
        public int purchased; // stack count

        public Augment(int id, string title, string description, float chance, bool repeatable)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.chance = chance;
            this.repeatable = repeatable;
            this.purchased = 0;
        }
    }

    public List<Augment> allAugments = new();
    private Dictionary<int, Augment> augmentById = new();

    protected override void OnAwake()
    {
        ResetAugments();
    }

    private void ResetAugments()
    {
        allAugments.Clear();
        augmentById.Clear();

        AddAugment(new Augment(0, "Vital Boost",
            "All power-ups additionally restore 10% health.", 5f, false));

        AddAugment(new Augment(1, "Enhanced Haste",
            "Haste power-up multiplier is increased by +0.5.", 15f, true));

        AddAugment(new Augment(2, "Fortified Shield",
            "Shield power-up duration and cooldown are increased by +0.5 seconds.", 15f, true));

        AddAugment(new Augment(3, "Overdrive Rush",
            "Rush power-up effect is increased to 3x.", 5f, false));

        AddAugment(new Augment(4, "Controlled Rage",
            "While Rage is active, damage taken is reduced to 25%.", 5f, false));

        AddAugment(new Augment(5, "Brutal Rage",
            "Rage power-up damage bonus is increased to +150%.", 5f, false));

        AddAugment(new Augment(6, "Golden Opportunity",
            "Gold power-up can be used +1 extra time per wave.", 20f, true));

        AddAugment(new Augment(7, "Expanded Destruction",
            "Area damage power-up radius is increased by 1.2x.", 15f, true));

        AddAugment(new Augment(8, "Improved Life Steal",
            "Life Steal power-up steal amount is increased by +5%.", 15f, true));
    }

    private void AddAugment(Augment augment)
    {
        allAugments.Add(augment);
        augmentById.Add(augment.id, augment);
    }

    public Augment GetAugmentById(int id)
    {
        augmentById.TryGetValue(id, out var augment);
        return augment;
    }

    public List<Augment> GetRandomAugments(int count)
    {
        List<Augment> pool = new();

        foreach (var augment in allAugments)
        {
            if (augment.repeatable || augment.purchased == 0)
                pool.Add(augment);
        }

        return GetWeightedSelection(pool, count);
    }

    private List<Augment> GetWeightedSelection(List<Augment> pool, int count)
    {
        List<Augment> result = new();
        List<Augment> tempPool = new(pool);

        for (int i = 0; i < count && tempPool.Count > 0; i++)
        {
            Augment chosen = GetWeightedRandom(tempPool);
            result.Add(chosen);

            if (!chosen.repeatable)
                tempPool.Remove(chosen);
        }

        return result;
    }

    private Augment GetWeightedRandom(List<Augment> pool)
    {
        float totalWeight = 0f;
        foreach (var augment in pool)
            totalWeight += augment.chance;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var augment in pool)
        {
            cumulative += augment.chance;
            if (roll <= cumulative)
                return augment;
        }

        return pool[^1];
    }

    public void PurchaseAugment(int augmentId)
    {
        Augment augment = GetAugmentById(augmentId);
        if (augment == null) return;

        augment.purchased++;
    }
}
