using UnityEngine;
using static Throwable;

public static class PowerUpEffectProcessor
{
    public static void Apply(PowerUpType type, Person person)
    {
        Debug.Log(
            $"[PowerUp APPLY] Type: {type}, " +
            $"Target: {person.name}, " +
            $"IsFriendly: {person.IsFriendly}"
        );
    }

}
