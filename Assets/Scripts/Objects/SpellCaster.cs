using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class SpellCaster : MonoBehaviour
{
    // --- Configuration ---
    public Text inputDisplay; // Reference to the UI Text where typed letters show
    public Text targetDisplay; // Reference to the UI Text showing the target spell word

    // Key: Spell Phrase, Value: Spell Name/Action ID
    private Dictionary<string, string> spellDictionary = new Dictionary<string, string>()
    {
        { "IGNIS", "FireballSpell" },
        { "AQUA", "HealSpell" },
        { "FULGUR", "LightningBolt" }
    };

    // --- Runtime State ---
    private string currentTargetPhrase = "";
    private string currentInputBuffer = "";

    void Start()
    {
        // Example: Start with a target spell (e.g., randomly selected or hotkeyed)
        SetTargetSpell("IGNIS");
    }

    void Update()
    {
        // Process keyboard input
        ProcessInput();

        // Update UI displays
        inputDisplay.text = currentInputBuffer;
    }

    private void SetTargetSpell(string phrase)
    {
        currentTargetPhrase = phrase.ToUpper(); // Ensure case consistency
        targetDisplay.text = currentTargetPhrase;
        currentInputBuffer = ""; // Clear buffer for new spell
    }

    private void ProcessInput()
    {
        // 1. Check for any key press this frame
        if (Input.inputString.Length > 0)
        {
            char lastChar = Input.inputString.ToUpper()[Input.inputString.Length - 1];

            // 2. Check if the typed character is valid for the current spell
            if (currentTargetPhrase.StartsWith(currentInputBuffer + lastChar))
            {
                // Correct character typed: Append it
                currentInputBuffer += lastChar;

                // 3. Check for successful cast
                if (currentInputBuffer == currentTargetPhrase)
                {
                    CastSpell();
                    currentInputBuffer = ""; // Reset buffer after cast
                }
            }
            else
            {
                // Incorrect character typed: Handle failure
                Debug.Log($"Failed to type {currentTargetPhrase}. Typed '{currentInputBuffer + lastChar}'.");
                // Reset or penalize the player
                currentInputBuffer = "";
            }
        }
    }

    private void CastSpell()
    {
        if (spellDictionary.TryGetValue(currentTargetPhrase, out string spellName))
        {
            Debug.Log($"--- Spell CAST: {spellName} ({currentTargetPhrase}) ---");

            // **Trigger the actual spell effect here**
            // Example: GetComponent<SpellEffectManager>().ExecuteSpell(spellName);

            // For now, just reset the target spell (e.g., pick a new one)
            SetTargetSpell("AQUA");
        }
    }
}