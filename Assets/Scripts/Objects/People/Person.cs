using UnityEngine;

public abstract class Person : MonoBehaviour
{
    [SerializeField] protected int health;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float damage;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected int aggroVal;
    [SerializeField] protected float damageArea;
    [SerializeField] protected bool isFriendly;
    [SerializeField] protected bool[] powerUpList = new bool[5]; // [strength, speed, shield, -, -]

    protected abstract void Start();
    protected abstract void Update();
    protected abstract void OnDestroy();

}
