using UnityEngine;

public abstract class Person : MonoBehaviour
{
    [SerializeField] protected int health;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float damage;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float aggroVal;
    [SerializeField] protected float damageArea;
    [SerializeField] protected bool isFriendly;
    [SerializeField] protected float givenXP;
    [SerializeField] protected int givenGold;
    [SerializeField] protected bool[] powerUpList = new bool[5]; // isFriendly = true: [strength, speed, shield, -, -] else: [goldIncrease, fatique, -, -, -]

    public float AggroVal => aggroVal;

    public bool IsFriendly => isFriendly;

    public Person TargetEntity;

    protected abstract void Start();
    protected abstract void Update();
    protected abstract void OnDestroy();
}