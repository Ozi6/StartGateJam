using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Person person;
    [SerializeField] private Image fillImage;
    [SerializeField] private Canvas canvas;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private Color friendlyColor = Color.green;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private float smoothSpeed = 5f;

    private Camera mainCamera;
    private float targetFillAmount;

    private void Awake()
    {
        mainCamera = Camera.main;

        // Auto-find Person component if not assigned
        if (person == null)
        {
            person = GetComponentInParent<Person>();
        }

        // Setup canvas for world space UI
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
    }

    private void Start()
    {
        if (person != null)
        {
            SetHealthBarColor(person.isFriendly);
            UpdateHealthBar();
        }
    }

    public void SetHealthBarColor(bool friendly)
    {
        if (fillImage != null)
        {
            fillImage.color = friendly ? friendlyColor : enemyColor;
        }
    }

    private void LateUpdate()
    {
        if (person == null || mainCamera == null) return;

        // Position health bar above unit
        transform.position = person.transform.position + offset;

        // Make health bar face camera
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);

        // Update health display
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (person == null || fillImage == null) return;

        // Calculate fill amount
        targetFillAmount = Mathf.Clamp01((float)person.Health / person.MaxHealth);

        // Smooth transition
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);

        // Hide when full (optional)
        if (hideWhenFull && canvas != null)
        {
            canvas.enabled = person.Health < person.MaxHealth;
        }
    }

    public void ForceUpdate()
    {
        if (person != null && fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01((float)person.Health / person.MaxHealth);
        }
    }
}