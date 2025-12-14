using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private UIManager uiManager;
    private KeywordDatabase database;
    private GameManager gameManager;

    [Header("Display Settings")]
    // 1. Assign an empty GameObject located in front of the player here in the Inspector
    public Transform heldItemDisplayPosition;
    public float heldItemRotationSpeed = 60f;

    public bool typerEnable;
    public int goldPowerSummonedCount = 0;

    private string currentBubbleText = "";
    private string heldKeyword = "";

    public void Initialize(UIManager ui, KeywordDatabase db, GameManager gm)
    {
        uiManager = ui;
        database = db;
        gameManager = gm;
    }

    void Update()
    {
        HandleSpecialKeys();

        if (typerEnable)
        {
            HandleKeyboardInput();
        }

        // 2. Rotate the held item around itself while it exists
        if (gameManager.currentThrowableHeld != null)
        {
            // We check if Space is NOT held so we don't fight with TrajectoryLauncher aiming rotation
            if (!Input.GetKey(KeyCode.Space))
            {
                gameManager.currentThrowableHeld.transform.Rotate(Vector3.up, heldItemRotationSpeed * Time.deltaTime);
            }
        }
    }

    public void Reset()
    {
        currentBubbleText = "";
        heldKeyword = "";
    }

    // ==============================
    // KEYBOARD INPUT
    // ==============================
    private void HandleKeyboardInput()
    {
        foreach (char rawChar in Input.inputString)
        {
            if (!char.IsLetter(rawChar)) continue;
            if (gameManager.currentThrowableHeld != null) continue;

            char normalizedChar = NormalizeChar(rawChar);

            if (string.IsNullOrEmpty(currentBubbleText))
                gameManager.playerAnimator.SetBool("IsWriting", true);

            currentBubbleText += normalizedChar;
            uiManager.UpdateSpeechBubble(currentBubbleText);

            if (database != null && database.IsValid(currentBubbleText))
            {
                string matched = currentBubbleText;
                currentBubbleText = "";
                uiManager.UpdateSpeechBubble("");
                gameManager.playerAnimator.SetBool("IsWriting", false);

                if (string.IsNullOrEmpty(heldKeyword))
                {
                    heldKeyword = matched;
                    uiManager.DisplayKeyword(heldKeyword);
                }
                else
                {
                    TryCreateThrowable(matched);
                }
            }
        }
    }

    // ==============================
    // SPECIAL KEYS
    // ==============================
    private void HandleSpecialKeys()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (!string.IsNullOrEmpty(currentBubbleText))
            {
                currentBubbleText = "";
                uiManager.UpdateSpeechBubble("");
            }
            else if (!string.IsNullOrEmpty(heldKeyword))
            {
                heldKeyword = "";
                uiManager.ClearHeldKeywordUI();
            }
            else if (gameManager.currentThrowableHeld != null)
            {
                Destroy(gameManager.currentThrowableHeld);
                GameManager.Instance.currentThrowableHeld = null;
                GameManager.Instance.playerAnimator.SetTrigger("Correct");
            }
            gameManager.playerAnimator.SetTrigger("Wrong");
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            uiManager.TogglePanel();
        }
    }

    // ==============================
    // THROWABLE CREATION
    // ==============================
    private void TryCreateThrowable(string secondKeyword)
    {
        if (database.HasPair(heldKeyword, secondKeyword))
        {
            PowerUpType ans = database.GetPowerUp(heldKeyword, secondKeyword);

            if (ans == PowerUpType.EnemyGold)
            {
                if (goldPowerSummonedCount < 1 + AugmentHandler.Instance.GetAugmentById(6).purchased)
                {
                    goldPowerSummonedCount++;
                }
                else
                {
                    uiManager.ResetSpells();
                    heldKeyword = "";
                    return;
                }
            }

            GameObject prefab = database.GetPrefab(heldKeyword, secondKeyword);
            if (prefab != null)
            {
                // 3. Instantiate at the specific display position
                gameManager.currentThrowableHeld = Instantiate(
                    prefab,
                    heldItemDisplayPosition != null ? heldItemDisplayPosition.position : Vector3.zero,
                    Quaternion.identity
                );

                // 4. Parent it to the display position so it follows the player automatically
                if (heldItemDisplayPosition != null)
                {
                    gameManager.currentThrowableHeld.transform.SetParent(heldItemDisplayPosition);
                    // Reset local position just in case
                    gameManager.currentThrowableHeld.transform.localPosition = Vector3.zero;
                }

                Rigidbody rb = gameManager.currentThrowableHeld.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                Throwable throwable = gameManager.currentThrowableHeld.GetComponent<Throwable>();
                if (throwable != null)
                {
                    throwable.powerUpType = database.GetPowerUp(heldKeyword, secondKeyword);
                }
            }
            uiManager.DisplayAndHideSecondKeyword(secondKeyword, 0.5f);
            heldKeyword = "";
        }
        else
        {
            heldKeyword = secondKeyword;
            uiManager.DisplayKeyword(heldKeyword);
        }
    }

    private char NormalizeChar(char c)
    {
        c = char.ToLowerInvariant(c);
        switch (c)
        {
            case 'ý': return 'i';
            case 'ð': return 'g';
            case 'þ': return 's';
            case 'ö': return 'o';
            case 'ü': return 'u';
            case 'ç': return 'c';
            default: return c;
        }
    }
}