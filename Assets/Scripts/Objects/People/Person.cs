using System.Collections;
using UnityEngine;

public abstract class Person : MonoBehaviour
{
    [Header("Collision Push")]
    private float pushStrength = 0.00002f;
    protected Rigidbody rb;
    [SerializeField] protected int maxHealth;
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
    [SerializeField] protected string poolTag;

    public bool isEnemyGolded = false;
    private float deploymentSpeed = 70;
    public bool IsFriendly => isFriendly;
    public int Health => health;

    public int MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public float Damage => damage;
    public float AttackRange => attackRange;
    public float AttackSpeed => attackSpeed;
    public float DamageArea => damageArea;
    public float GivenXP => givenXP;
    public int GivenGold => givenGold;
    public bool[] PowerUpList => powerUpList;
    public Person TargetEntity;
    protected bool isWaiting = false;
    protected float lastAttackTime = -Mathf.Infinity;
    public Vector3 targetPosition;
    public void OnObjectSpawn()
    {
        UnitRegistrar.RegisterUnit(this);
    }
    public void OnObjectReturn()
    {
        UnitRegistrar.UnregisterUnit(this);
    }
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints =
            RigidbodyConstraints.FreezeRotation |
            RigidbodyConstraints.FreezePositionY;
    }
    protected abstract void Start();
    protected virtual void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.Deployment)
        {
            if (isWaiting && Vector3.Distance(transform.position, targetPosition) > 0.01f)
                GoToPointDesignatedVer(targetPosition);
        }
        else if (GameManager.Instance.CurrentState == GameState.Combat)
        {
            if (isWaiting)
            {
                if (TargetEntity != null && TargetEntity.gameObject.activeSelf)
                {
                    Engage();
                }
                else
                {
                    TargetEntity = TeamTargetManager.Instance.GetNewTarget(this);
                    if (TargetEntity != null && TargetEntity.gameObject.activeSelf)
                    {
                        Engage();
                    }
                    else
                    {
                        StopMoving();
                    }
                }
            }
        }
    }
    protected abstract void OnDestroy();
    protected void GoToPointDesignatedVer(Vector3 point)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            point,
            deploymentSpeed * Time.deltaTime
        );
    }
    protected void GoToPoint(Vector3 point)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            point,
            moveSpeed * Time.deltaTime
        );
    }
    protected void StopMoving()
    {
        rb.linearVelocity = Vector3.zero;
    }
    public void BeginDeployment(Vector3 designated)
    {
        StartCoroutine(MoveToDesignated(designated));
    }
    protected IEnumerator MoveToDesignated(Vector3 designated)
    {
        while (Vector3.Distance(transform.position, designated) > 0.01f)
        {
            GoToPointDesignatedVer(designated);
            yield return null;
        }
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            GoToPointDesignatedVer(targetPosition);
            yield return null;
        }
        StopMoving();
        isWaiting = true;
    }
    protected virtual void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer != gameObject.layer)
            return;
        Person other = collision.gameObject.GetComponent<Person>();
        if (other == null)
            return;
        Vector3 pushDir = transform.position - other.transform.position;
        pushDir.y = 0f;
        if (pushDir.sqrMagnitude < 0.0001f)
            return;
        //rb.AddForce(pushDir.normalized * pushStrength, ForceMode.Acceleration);
    }
    protected virtual void Engage()
    {
        if (TargetEntity == null || !TargetEntity.gameObject.activeSelf) return;
        float distance = Vector3.Distance(transform.position, TargetEntity.transform.position);
        if (distance > attackRange)
        {
            GoToPoint(TargetEntity.transform.position);
        }
        else
        {
            StopMoving();
            if (Time.time >= lastAttackTime + attackSpeed)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }
    protected virtual void Attack()
    {
        if (TargetEntity != null && TargetEntity.gameObject.activeSelf)
        {
            TargetEntity.TakeDamage(CalculateDamage());
        }
    }
    public virtual float CalculateDamage()
    {
        return damage;
    }
    public virtual void TakeDamage(float dmg)
    {
        health -= Mathf.RoundToInt(dmg);
        if(health > maxHealth)
        {
            health = maxHealth;
        }
        else if (health <= 0)
        {
            Die();
        }
    }
    protected virtual void Die()
    {
        ObjectPooler.Instance.ReturnToPool(gameObject, poolTag);
        GameManager.Instance.currentGold += isEnemyGolded ? givenGold+1 : givenGold ;
    }
}