public static class UnitRegistrar
{
    public static void RegisterUnit(Person person)
    {
        if (GameManager.Instance == null)
            return;
        if (person.IsFriendly)
        {
            if (!GameManager.Instance.playersTeam.Contains(person))
                GameManager.Instance.playersTeam.Add(person);
        }
        else
        {
            if (!GameManager.Instance.enemyTeam.Contains(person))
                GameManager.Instance.enemyTeam.Add(person);
        }
    }

    public static void UnregisterUnit(Person person)
    {
        if (GameManager.Instance == null) return;
        if (person.IsFriendly)
            GameManager.Instance.playersTeam.Remove(person);
        else
            GameManager.Instance.enemyTeam.Remove(person);
    }
}