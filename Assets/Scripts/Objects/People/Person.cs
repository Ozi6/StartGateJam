using System.Collections;
using UnityEngine;

public abstract class Person : MonoBehaviour
{
    [Header("Collision Push")]
    protected Rigidbody rb;
    [SerializeField] public int maxHealth;
    [SerializeField] public int health;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float damage;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float damageArea;
    [SerializeField] public bool isFriendly;
    [SerializeField] protected float givenXP;
    [SerializeField] protected int givenGold;
    [SerializeField] protected string poolTag;
    [SerializeField] protected float areaBuff;
    [SerializeField] protected float turnSpeed = 10f;
    public int upgradeCost;
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
    public float AreaBuff => areaBuff;
    public int GivenGold => givenGold;
    public Person TargetEntity;
    protected bool isWaiting = false;
    protected float lastAttackTime = -Mathf.Infinity;
    public Vector3 targetPosition;
    [SerializeField] HealthBar healthBar;

    public void OnObjectSpawn()
    {
        UnitRegistrar.RegisterUnit(this);
    }

    public void OnObjectReturn()
    {

    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ |
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

    protected abstract void OnDestroy();

    protected void GoToPointDesignatedVer(Vector3 point)
    {
        FacePosition(point);
        Vector3 newPos = Vector3.MoveTowards(transform.position, point, deploymentSpeed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    protected void GoToPoint(Vector3 point)
    {
        if (!CanMove) return;
        FacePosition(point);
        Vector3 newPos = Vector3.MoveTowards(transform.position, point, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    protected void StopMoving()
    {
        rb.linearVelocity = Vector3.zero;
    }

    public void BeginDeployment(Vector3 designated)
    {
        StartCoroutine(MoveToDesignated(designated));
    }

    public void SetFriendly(bool friendly)
    {
        isFriendly = friendly;
        healthBar.SetHealthBarColor(friendly);
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
        FacePosition(TargetEntity.transform.position);
        float distance = Vector3.Distance(transform.position, TargetEntity.transform.position);
        if (distance > attackRange)
        {
            GoToPoint(TargetEntity.transform.position);
        }
        else
        {
            StopMoving();
            if (Time.time >= lastAttackTime + (1/attackSpeed))
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    protected virtual void Attack()
    {
        if (TargetEntity == null || !TargetEntity.gameObject.activeSelf)
            return;
        float finalDamage = CalculateDamage();
        if (HasAreaDamage && damageArea > 0f)
        {
            ApplyAreaDamage(TargetEntity.transform.position, finalDamage);
        }
        else
        {
            TargetEntity.TakeDamage(finalDamage);
        }
    }

    protected virtual void ApplyAreaDamage(Vector3 center, float dmg)
    {
        Collider[] hits = Physics.OverlapSphere(center, damageArea);
        foreach (Collider hit in hits)
        {
            Person p = hit.GetComponent<Person>();
            if (p == null) continue;
            // Skip same team
            if (p.IsFriendly == this.IsFriendly) continue;
            // Skip dead / inactive
            if (!p.gameObject.activeSelf) continue;
            p.TakeDamage(dmg);
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
        UnitRegistrar.UnregisterUnit(this);
        GameManager.Instance.currentGold += isEnemyGolded ? givenGold + 1 : givenGold;
    }

    protected void FacePosition(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(0, 180, 0); // Adjust for backwards model
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    // ----------- COROUTINES --------------
    public void ApplyShield(float duration)
    {
        float augmentalLag = 0.5f * AugmentHandler.Instance.GetAugmentById(1).purchased;
        StartCoroutine(ShieldRoutine(duration + augmentalLag));
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
        if (AugmentHandler.Instance.GetAugmentById(3).purchased > 0)
        {
            multiplier = 3f;
        }
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
        float augmentalMult = 0.5f * AugmentHandler.Instance.GetAugmentById(1).purchased;
        StartCoroutine(HasteRoutine(multiplier + augmentalMult, duration));
    }

    private IEnumerator HasteRoutine(float multiplier, float duration)
    {
        attackSpeed *= multiplier; // faster attacks
        yield return new WaitForSeconds(duration);
        attackSpeed /= multiplier;
    }

    public void ApplyRage(float dmgMult, float takenMult, float duration)
    {
        if (AugmentHandler.Instance.GetAugmentById(4).purchased > 0)
        {
            takenMult = 1.25f;
        }
        if (AugmentHandler.Instance.GetAugmentById(5).purchased > 0)
        {
            dmgMult = 2.5f;
        }
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

    public void ApplyAreaDamage(float duration)
    {
        float radiusMultAugment = 0.2f * AugmentHandler.Instance.GetAugmentById(7).purchased;
        StartCoroutine(AreaDamageRoutine(duration, areaBuff + radiusMultAugment));
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
        float lifeStealMult = 0.5f * AugmentHandler.Instance.GetAugmentById(8).purchased;
        StartCoroutine(LifeStealRoutine(duration + lifeStealMult));
    }

    private IEnumerator LifeStealRoutine(float duration)
    {
        HasLifeSteal = true;
        yield return new WaitForSeconds(duration);
        HasLifeSteal = false;
    }

    public void GetStats(
        out int maxHp,
        out int hp,
        out float moveSpd,
        out float dmg,
        out float atkRange,
        out float atkSpd,
        out float dmgArea,
        out float dmgMult,
        out float dmgTakenMult,
        out bool invulnerable,
        out bool canMove,
        out bool areaDamage,
        out bool lifeSteal)
    {
        maxHp = maxHealth;
        hp = health;
        moveSpd = moveSpeed;
        dmg = damage;
        atkRange = attackRange;
        atkSpd = attackSpeed;
        dmgArea = damageArea;
        dmgMult = damageMultiplier;
        dmgTakenMult = damageTakenMultiplier;
        invulnerable = IsInvulnerable;
        canMove = CanMove;
        areaDamage = HasAreaDamage;
        lifeSteal = HasLifeSteal;
    }
}