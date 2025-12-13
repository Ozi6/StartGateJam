using System.Collections;
using UnityEngine;

public abstract class Person : MonoBehaviour
{
    [Header("Collision Push")]
    [SerializeField] protected float pushStrength = 0.5f;

    protected Rigidbody rb;

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
    public Vector3 targetPosition;
    protected bool isWaiting = false;

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
        if (isWaiting && Vector3.Distance(transform.position, targetPosition) > 0.01f)
            GoToPoint(targetPosition);
    }

    protected abstract void OnDestroy();

    protected void GoToPoint(Vector3 point)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            point,
            moveSpeed * Time.deltaTime
        );
    }

    public void BeginDeployment(Vector3 designated)
    {
        StartCoroutine(MoveToDesignated(designated));
    }

    protected IEnumerator MoveToDesignated(Vector3 designated)
    {
        while (Vector3.Distance(transform.position, designated) > 0.01f)
        {
            GoToPoint(designated);
            yield return null;
        }
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

        rb.AddForce(pushDir.normalized * pushStrength, ForceMode.Acceleration);
    }
}