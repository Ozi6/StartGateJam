using System;
using UnityEngine;
using static Throwable;

public static class PowerUpEffectProcessor
{
    public static void Apply(PowerUpType type, Person person)
    {
        if (AugmentHandler.Instance.allAugments[0].purchased == 1)
        {
            float x = person.MaxHealth * 0.1f;
            person.TakeDamage(-x);
        }
        Debug.Log(
            $"[PowerUp APPLY] Type: {type}, " +
            $"Target: {person.name}, " +
            $"IsFriendly: {person.IsFriendly}"
        );
    }

}
