using UnityEngine;

public static class PowerUpEffectProcessor
{
    public static void Apply(PowerUpType type, Person person)
    {
        Debug.Log($"[PowerUp APPLY] {type} -> {person.name} : {person.isFriendly}");

        if (person.IsFriendly)
        {
            if (AugmentHandler.Instance.GetAugmentById(0).purchased > 0)
            {
                float x = person.MaxHealth * 0.1f;
                person.TakeDamage(-x);
            }
            switch (type)
            {
                case PowerUpType.Shield:
                    person.ApplyShield(2f);
                    break;

                case PowerUpType.Rush:
                    person.ApplyRush(2f, 3f);
                    break;

                case PowerUpType.Haste:
                    person.ApplyHaste(1.5f, 4f);
                    break;

                case PowerUpType.Rage:
                    person.ApplyRage(2f, 1.5f, 4f);
                    break;

                case PowerUpType.AreaDamage:
                    person.ApplyAreaDamage(4f, 1.2f);
                    break;

                case PowerUpType.LifeSteal:
                    person.ApplyLifeSteal(2f);
                    break;
            }
        }
        else if (type == PowerUpType.EnemyGold)
        {
            person.isEnemyGolded = true;
        }
    }
}
