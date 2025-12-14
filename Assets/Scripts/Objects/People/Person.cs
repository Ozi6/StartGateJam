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
    [SerializeField] public int upgradeCost;
    public bool isNew = false;
    // --- WALK SFX ADDITIONS ---
    [Header("Walk SFX")]
    [SerializeField] protected float walkSFXInterval = 0.4f; // Time between walk sounds
    private float walkSFXTimer = 0f;
    // -------------------------
    [Header("Ground Detection")]
    [SerializeField] protected float groundRaycastDistance = 10f;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected float groundOffset = 0.1f; // Small offset above ground
    // ---------- POWER-UP STATES ----------
    public bool IsInvulnerable { get; private set; }
    public bool CanMove { get; private set; } = true;
    public bool HasAreaDamage { get; private set; }
    public bool HasLifeSteal { get; private set; }
    private float damageMultiplier = 1f;
    private float damageTakenMultiplier = 1f;
    public int EnemyGolded = 0;
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
    [SerializeField] protected Animator animator;
    [SerializeField] ParticleSystem sandEffect;
    // ---------- NEW: ATTACK DURATION ----------
    [Header("Attack Settings")]
    [SerializeField] protected float attackDuration = 0.5f; // Duration of the attack action (adjust as needed)
    protected bool isAttacking = false;

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
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ |
            RigidbodyConstraints.FreezePositionY;
    }

    protected abstract void Start();

    protected virtual void Update()
    {
        bool isMoving = false; // Flag to check if the unit is moving this frame
        if (GameManager.Instance.CurrentState == GameState.Deployment)
        {
            if (Vector3.Distance(transform.position, targetPosition) > 0.01f && !isFriendly)
            {
                GoToPointDesignatedVer(targetPosition);
                isMoving = true;
            }
        }
        else if (GameManager.Instance.CurrentState == GameState.Combat)
        {
            if (TargetEntity != null && TargetEntity.gameObject.activeSelf)
            {
                isMoving = Engage(); // Engage returns true if movement occurred
            }
            else
            {
                TargetEntity = TeamTargetManager.Instance.GetNewTarget(this);
                if (TargetEntity != null && TargetEntity.gameObject.activeSelf)
                {
                    isMoving = Engage(); // Engage returns true if movement occurred
                }
                else
                {
                    StopMoving();
                }
            }
        }
        // Handle Walk SFX
        if (isMoving)
        {
            walkSFXTimer += Time.deltaTime;
            if (walkSFXTimer >= walkSFXInterval)
            {
                PlayWalkSFX();
                walkSFXTimer = 0f;
            }
        }
        else
        {
            walkSFXTimer = 0f; // Reset timer when stopped
        }
        // Always lock to ground
        LockToGround();
    }

    protected abstract void OnDestroy();

    protected void LockToGround()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 5f; // Start raycast from above
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundRaycastDistance, groundLayer))
        {
            float targetY = hit.point.y + groundOffset;
            Vector3 pos = transform.position;
            pos.y = targetY;
            transform.position = pos;
        }
    }

    protected void GoToPointDesignatedVer(Vector3 point)
    {
        FacePosition(point);
        if (!sandEffect.isPlaying) sandEffect.Play();
        // Only move in XZ plane
        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(point.x, currentPos.y, point.z);
        Vector3 newPos = Vector3.MoveTowards(currentPos, targetPos, deploymentSpeed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    protected void GoToPoint(Vector3 point)
    {
        if (!CanMove) return;
        FacePosition(point);
        if (!sandEffect.isPlaying) sandEffect.Play();
        // Only move in XZ plane
        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(point.x, currentPos.y, point.z);
        Vector3 newPos = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    protected void StopMoving()
    {
        rb.linearVelocity = Vector3.zero;
        sandEffect.Stop();
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
        while (Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(designated.x, designated.z)
        ) > 0.01f)
        {
            GoToPointDesignatedVer(designated);
            yield return new WaitForFixedUpdate();
        }
        while (Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(targetPosition.x, targetPosition.z)
        ) > 0.01f)
        {
            GoToPointDesignatedVer(targetPosition);
            yield return new WaitForFixedUpdate();
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

    // Changed return type to bool to indicate if movement happened
    protected virtual bool Engage()
    {
        if (TargetEntity == null || !TargetEntity.gameObject.activeSelf) return false;
        FacePosition(TargetEntity.transform.position);

        if (isAttacking)
        {
            StopMoving();
            return false; // No movement while attacking
        }

        float distance = Vector3.Distance(transform.position, TargetEntity.transform.position);
        if (distance > attackRange)
        {
            GoToPoint(TargetEntity.transform.position);
            return true;
        }
        else
        {
            StopMoving();
            if (Time.time >= lastAttackTime + (1 / attackSpeed))
            {
                StartCoroutine(PerformAttack());
                lastAttackTime = Time.time;
            }
            return false;
        }
    }

    protected virtual IEnumerator PerformAttack()
    {
        isAttacking = true;
        if (animator != null)
            animator.SetBool("Attacking", true);

        yield return new WaitForSeconds(attackDuration / 2f);

        if (TargetEntity != null && TargetEntity.gameObject.activeSelf)
        {
            float finalDamage = CalculateDamage();
            if (HasAreaDamage && damageArea > 0f)
            {
                ApplyAreaDamage(TargetEntity.transform.position, finalDamage);
            }
            else
            {
                TargetEntity.TakeDamage(finalDamage);
            }
            AttackSound();
        }

        // Wait for the remaining duration to complete the attack
        yield return new WaitForSeconds(attackDuration / 2f);

        if (animator != null)
            animator.SetBool("Attacking", false);
        isAttacking = false;
    }

    protected virtual void AttackSound()
    {
    }

    protected virtual void PlayWalkSFX()
    {
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

    [SerializeField] protected ParticleSystem hurtParticleSystem;

    public virtual void TakeDamage(float dmg)
    {
        if (IsInvulnerable) return;

        dmg *= damageTakenMultiplier;

        int delta = Mathf.RoundToInt(dmg);
        if (delta > 0 && hurtParticleSystem != null)
        {
            hurtParticleSystem.Play();
        }

        health -= delta;

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
        if (!this.isFriendly)
        {
            int goldAmount = givenGold + EnemyGolded;
            GameManager.Instance.currentGold += goldAmount;
            Debug.Log("Enemy died and dropped " + givenGold + "base" + EnemyGolded + " buffed golds");
        }
        UnitRegistrar.UnregisterUnit(this);
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
        ShowPowerUpVisual(PowerUpType.Shield);
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
        ShowPowerUpVisual(PowerUpType.Rush);
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
        ShowPowerUpVisual(PowerUpType.Haste);
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
        ShowPowerUpVisual(PowerUpType.Rage);
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
        ShowPowerUpVisual(PowerUpType.AreaDamage);
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
        ShowPowerUpVisual(PowerUpType.LifeSteal);
        float lifeStealMult = 0.5f * AugmentHandler.Instance.GetAugmentById(8).purchased;
        StartCoroutine(LifeStealRoutine(duration + lifeStealMult));
    }

    private IEnumerator LifeStealRoutine(float duration)
    {
        HasLifeSteal = true;
        yield return new WaitForSeconds(duration);
        HasLifeSteal = false;
    }

    private void ShowPowerUpVisual(PowerUpType powerUpType)
    {
        KeywordDatabase db = GameManager.Instance.database;
        GameObject prefab = db.GetPrefabByPowerUpType(powerUpType);
        if (prefab == null) return;
        GameObject instance = Instantiate(prefab, transform.position + Vector3.up * 2f, Quaternion.identity);
        instance.transform.SetParent(transform);
        Collider col = instance.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Rigidbody instanceRb = instance.GetComponent<Rigidbody>();
        if (instanceRb != null)
        {
            instanceRb.isKinematic = true;
            instanceRb.useGravity = false;
        }
        Throwable throwable = instance.GetComponent<Throwable>();
        if (throwable != null)
        {
            throwable.OnThrown();
        }
        Destroy(instance, 1f);
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