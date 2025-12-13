using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryLauncher : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float maxRotationAngle = 45f;
    [SerializeField] private float launchAngle = 45f;

    [Header("Power Settings")]
    [SerializeField] private float minForce = 3f;
    [SerializeField] private float maxForce = 15f;
    [SerializeField] private float maxChargeTime = 2f;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform arrowVisual;
    [SerializeField] private int linePoints = 30;
    [SerializeField] private float timeStep = 0.1f;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform followObject;

    private Vector3 followOriginalPos;

    private int yawDirection = 1;

    private float currentYaw = 0f;
    private float currentChargeTime = 0f;
    private bool isCharging = false;
    private LineRenderer lineRenderer;

    private short chargeDirection = 1;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false;
        if (followObject != null)
            followOriginalPos = followObject.position;
    }

    private void Update()
    {
        HandleRotation();
        HandleShooting();
    }

    private void HandleRotation()
    {
        if (isCharging) return;
        currentYaw += yawDirection * rotationSpeed * Time.deltaTime;
        if (currentYaw >= maxRotationAngle)
            yawDirection = -1;
        else if (currentYaw <= -maxRotationAngle)
            yawDirection = 1;
        transform.localRotation = Quaternion.Euler(90, currentYaw, 0);
    }

    private void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GameManager.Instance.currentThrowableHeld == null) return;
            isCharging = true;
            currentChargeTime = 0f;
            lineRenderer.enabled = true;
        }
        if (isCharging && Input.GetKey(KeyCode.Space))
        {
            currentChargeTime += chargeDirection * Time.deltaTime;
            if (currentChargeTime >= maxChargeTime)
            {
                currentChargeTime = maxChargeTime;
                chargeDirection = -1;
            }
            else if (currentChargeTime <= 0f)
            {
                currentChargeTime = 0f;
                chargeDirection = 1;
            }
            DrawTrajectory(CalculateCurrentForce());

            GameObject held = GameManager.Instance.currentThrowableHeld;
            if (held != null)
            {
                Rigidbody rb = held.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.position = firePoint.position;
                    rb.rotation = firePoint.rotation;
                }
            }
        }
        else if (isCharging && Input.GetKeyUp(KeyCode.Space))
        {
            ThrowObject(CalculateCurrentForce());
            ResetLauncher();
        }
    }

    private void DrawTrajectory(float force)
    {
        Vector3[] points = new Vector3[linePoints];
        Vector3 startingVelocity = GetTrajectoryVelocity(force);
        Vector3 startingPosition = firePoint.position;

        lineRenderer.positionCount = 0;

        Vector3 prevPoint = startingPosition;
        float time = 0f;

        for (int i = 0; i < linePoints; i++)
        {
            time += timeStep;
            Vector3 movementVector = startingVelocity * time;
            Vector3 gravityVector = 0.5f * Physics.gravity * time * time;
            Vector3 nextPoint = startingPosition + movementVector + gravityVector;
            if (Physics.Raycast(prevPoint, (nextPoint - prevPoint).normalized,
                out RaycastHit hit, (nextPoint - prevPoint).magnitude, groundLayer))
            {
                lineRenderer.positionCount = i + 1;
                points[i] = hit.point;
                lineRenderer.SetPositions(points);
                if (followObject != null)
                {
                    Vector3 endPoint = points[lineRenderer.positionCount - 1];
                    followObject.position = endPoint;
                }
                return;
            }
            points[i] = nextPoint;
            prevPoint = nextPoint;
        }
        lineRenderer.positionCount = linePoints;
        lineRenderer.SetPositions(points);
        if (followObject != null)
        {
            Vector3 endPoint = points[lineRenderer.positionCount - 1];
            followObject.position = endPoint;
        }
    }

    private void ThrowObject(float force)
    {
        GameObject held = GameManager.Instance.currentThrowableHeld;
        if (held != null)
        {
            Rigidbody rb = held.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.transform.position = firePoint.position;
                rb.transform.rotation = firePoint.rotation;
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = GetLaunchVelocity(force);
                rb.angularVelocity = Vector3.zero;
            }
            Throwable throwable = held.GetComponent<Throwable>();
            if (throwable != null)
                throwable.OnThrown();
            GameManager.Instance.currentThrowableHeld = null;
        }
    }

    private Vector3 GetLaunchVelocity(float force)
    {
        return GetTrajectoryVelocity(force);
    }

    private Vector3 GetTrajectoryVelocity(float force)
    {
        float radians = launchAngle * Mathf.Deg2Rad;
        Vector3 horizontalDir = Quaternion.Euler(0f, currentYaw, 0f) * Vector3.forward;
        Vector3 launchDirection =
            horizontalDir * Mathf.Cos(radians) +
            Vector3.up * Mathf.Sin(radians);
        return launchDirection.normalized * force;
    }

    private float CalculateCurrentForce()
    {
        float chargeRatio = Mathf.Clamp01(currentChargeTime / maxChargeTime);
        return Mathf.Lerp(minForce, maxForce, chargeRatio);
    }

    private void ResetLauncher()
    {
        isCharging = false;
        currentChargeTime = 0f;
        lineRenderer.enabled = false;
        if (followObject != null)
            followObject.position = followOriginalPos;
    }
}