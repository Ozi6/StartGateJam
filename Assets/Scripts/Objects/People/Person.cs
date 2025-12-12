using UnityEngine;

public abstract class Person : MonoBehaviour
{
    [SerializeField] protected int health;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float damage;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float damageArea;
    [SerializeField] protected bool isFriendly;
    [SerializeField] protected float givenXP;
    [SerializeField] protected int givenGold;
    [SerializeField] protected bool[] powerUpList = new bool[5]; // isFriendly = true: [strength, speed, shield, -, -] else: [goldIncrease, fatique, -, -, -]


    public bool IsFriendly => isFriendly;
    public int Health => health;
    public float MoveSpeed => moveSpeed;
    public float Damage => damage;
    public float AttackRange => attackRange;
    public float AttackSpeed => attackSpeed;

    public float DamageArea => damageArea;

    public float GivenXP => givenXP;

    public float GivenGold => givenGold;

    public bool[] PowerUpList => powerUpList;


    public Person TargetEntity;

    public void OnObjectSpawn()
    {
        UnitRegistrar.RegisterUnit(this);
    }

    public void OnObjectReturn()
    {
        UnitRegistrar.UnregisterUnit(this);
    }

    protected abstract void Start();
    protected abstract void Update();
    protected abstract void OnDestroy();
}