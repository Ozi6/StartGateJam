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
    [SerializeField] protected string poolTag;

    // ---------- POWER-UP STATES ----------
    public bool IsInvulnerable { get; private set; }
    public bool CanMove { get; private set; } = true;

    public bool HasAreaDamage { get; private set; }
    public bool HasLifeSteal { get; private set; }

    private float damageMultiplier = 1f;
    private float damageTakenMultiplier = 1f;


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
        if (!CanMove) return;

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
        float finalDamage = damage * damageMultiplier;

        if (HasLifeSteal)
        {
            float heal = finalDamage * 0.35f;
            TakeDamage(-heal);
        }

        return finalDamage;
    }

    public virtual void TakeDamage(float dmg)
    {
        health -= Mathf.RoundToInt(dmg);
        if (IsInvulnerable) return;

        dmg *= damageTakenMultiplier;

        health -= Mathf.RoundToInt(dmg);

        if (health > maxHealth)
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
        GameManager.Instance.currentGold += isEnemyGolded ? givenGold + 1 : givenGold;
    }

    // ----------- COROUTINES --------------

    public void ApplyShield(float duration)
    {
        StartCoroutine(ShieldRoutine(duration));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        IsInvulnerable = true;
        CanMove = false;

        yield return new WaitForSeconds(duration);

        IsInvulnerable = false;
        CanMove = true;
    }


    public void ApplyRush(float multiplier, float duration)
    {
        StartCoroutine(RushRoutine(multiplier, duration));
    }

    private IEnumerator RushRoutine(float multiplier, float duration)
    {
        moveSpeed *= multiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed /= multiplier;
    }


    public void ApplyHaste(float multiplier, float duration)
    {
        StartCoroutine(HasteRoutine(multiplier, duration));
    }

    private IEnumerator HasteRoutine(float multiplier, float duration)
    {
        attackSpeed /= multiplier; // faster attacks
        yield return new WaitForSeconds(duration);
        attackSpeed *= multiplier;
    }


    public void ApplyRage(float dmgMult, float takenMult, float duration)
    {
        StartCoroutine(RageRoutine(dmgMult, takenMult, duration));
    }

    private IEnumerator RageRoutine(float dmgMult, float takenMult, float duration)
    {
        damageMultiplier *= dmgMult;
        damageTakenMultiplier *= takenMult;

        yield return new WaitForSeconds(duration);

        damageMultiplier /= dmgMult;
        damageTakenMultiplier /= takenMult;
    }


    public void ApplyAreaDamage(float duration, float radiusMultiplier)
    {
        StartCoroutine(AreaDamageRoutine(duration, radiusMultiplier));
    }

    private IEnumerator AreaDamageRoutine(float duration, float radiusMultiplier)
    {
        HasAreaDamage = true;
        damageArea *= radiusMultiplier;

        yield return new WaitForSeconds(duration);

        damageArea /= radiusMultiplier;
        HasAreaDamage = false;
    }

    public void ApplyLifeSteal(float duration)
    {
        StartCoroutine(LifeStealRoutine(duration));
    }

    private IEnumerator LifeStealRoutine(float duration)
    {
        HasLifeSteal = true;
        yield return new WaitForSeconds(duration);
        HasLifeSteal = false;
    }
}