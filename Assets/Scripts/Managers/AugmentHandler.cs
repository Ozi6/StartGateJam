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
        public int purchased;

        public Augment(int id, string title, string description, float chance, bool repeatable, int purchased)
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
    private List<Augment> availableAugments = new();

    private void Awake()
    {
        ResetAugmentPool();
    }


    public void ResetAugmentPool()
    {
        allAugments.Clear();

        allAugments.Add(new Augment(0, "Vital Boost",
            "All power-ups additionally restore 10% health.", 5f, true, 0));

        allAugments.Add(new Augment(1, "Enhanced Haste",
            "Haste power-up multiplier is increased by +0.5.", 15f, true, 0));

        allAugments.Add(new Augment(2, "Fortified Shield",
            "Shield power-up duration and cooldown are increased by +0.5 seconds.", 15f, true, 0));

        allAugments.Add(new Augment(3, "Overdrive Rush",
            "Rush power-up effect is increased to 3x.", 5f, false, 0));

        allAugments.Add(new Augment(4, "Controlled Rage",
            "While Rage is active, damage taken is reduced to 25%.", 5f, false, 0));

        allAugments.Add(new Augment(5, "Brutal Rage",
            "Rage power-up damage bonus is increased to +150%.", 5f, false, 0));

        allAugments.Add(new Augment(6, "Golden Opportunity",
            "Gold power-up can be used +1 extra time per wave.", 20f, true, 0));

        allAugments.Add(new Augment(7, "Expanded Destruction",
            "Area damage power-up radius is increased by 1.2x.", 15f, true, 0));

        allAugments.Add(new Augment(8, "Improved Life Steal",
            "Life Steal power-up steal amount is increased by +5%.", 15f, true, 0));
        availableAugments = new List<Augment>(allAugments);
    }

    public List<Augment> GetRandomAugments(int count)
    {
        List<Augment> result = new();
        List<Augment> tempPool = new(availableAugments);

        for (int i = 0; i < count && tempPool.Count > 0; i++)
        {
            Augment chosen = GetWeightedRandom(tempPool);
            result.Add(chosen);
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

    public void PurchaseAugment(Augment augment)
    {
        ApplyAugmentEffect(augment);

        if (!augment.repeatable)
        {
            availableAugments.RemoveAll(a => a.id == augment.id);
            Debug.Log($"Removed non-repeatable augment: {augment.title}");
        }

        allAugments[augment.id].purchased++;
    }

    private void ApplyAugmentEffect(Augment augment)
    {
        Debug.Log($"Applied augment: {augment.title}");
    }
}
