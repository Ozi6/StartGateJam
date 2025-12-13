using UnityEngine;

public class InputManager : MonoBehaviour
{
    private UIManager uiManager;
    private KeywordDatabase database;
    private GameManager gameManager;

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
        HandleKeyboardInput();
        HandleSpecialKeys();
    }

    private void HandleKeyboardInput()
    {
        foreach (char c in Input.inputString)
        {
            if (char.IsLetter(c))
            {
                if (gameManager.currentThrowableHeld != null) continue;

                currentBubbleText += char.ToLower(c);
                uiManager.UpdateSpeechBubble(currentBubbleText);

                if (database != null && database.IsValid(currentBubbleText))
                {
                    string matched = currentBubbleText;
                    currentBubbleText = "";
                    uiManager.UpdateSpeechBubble(currentBubbleText);

                    if (heldKeyword == "")
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
    }

    private void HandleSpecialKeys()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (!string.IsNullOrEmpty(currentBubbleText))
            {
                currentBubbleText = "";
                uiManager.UpdateSpeechBubble(currentBubbleText);
            }
            else if (heldKeyword != "")
            {
                heldKeyword = "";
                uiManager.ClearHeldKeywordUI();
            }
            else if (gameManager.currentThrowableHeld != null)
            {
                Destroy(gameManager.currentThrowableHeld);
                gameManager.currentThrowableHeld = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            uiManager.TogglePanel();
        }
    }

    private void TryCreateThrowable(string secondKeyword)
    {
        if (database.HasPair(heldKeyword, secondKeyword))
        {
            GameObject prefab = database.GetPrefab(heldKeyword, secondKeyword);
            if (prefab != null)
            {
                gameManager.currentThrowableHeld = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                Rigidbody rb = gameManager.currentThrowableHeld.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
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
}