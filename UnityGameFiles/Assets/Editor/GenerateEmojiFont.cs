using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// Unity Editor script to help configure emoji support for Sketch-Blossom game.
/// Provides multiple methods to add emoji support to TextMeshPro.
///
/// Usage: In Unity Editor, go to Tools > Configure Emoji Support
/// </summary>
public class GenerateEmojiFont : MonoBehaviour
{
    [MenuItem("Tools/Generate Emoji Font Asset")]
    public static void CreateEmojiFont()
    {
        Debug.Log("========================================");
        Debug.Log("GENERATING EMOJI FONT ASSET");
        Debug.Log("========================================\n");

        // Try to load the Noto Emoji fonts
        Font emojiFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/NotoEmoji-VariableFont_wght.ttf");

        if (emojiFont == null)
        {
            emojiFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/NotoEmoji-Regular.ttf");
        }

        if (emojiFont == null)
        {
            Debug.LogError("ERROR: Could not find NotoEmoji font files!");
            Debug.LogError("Expected location: Assets/TextMesh Pro/Fonts/");
            Debug.LogError("Please make sure NotoEmoji-VariableFont_wght.ttf or NotoEmoji-Regular.ttf is in that folder.");
            return;
        }

        Debug.Log($"✓ Found emoji font: {emojiFont.name}");

        // Define all emojis used in the game
        string emojiChars = "🔥🌿💧❌❓★☆❤⚔🛡⚡🌱🎨✓○";

        Debug.Log($"Creating font asset with {emojiChars.Length} emoji characters...");
        Debug.Log($"Characters: {emojiChars}\n");

        Debug.Log("========================================");
        Debug.Log("MANUAL STEPS REQUIRED:");
        Debug.Log("========================================");
        Debug.Log("Unity's TextMeshPro Font Asset Creator must be used manually.\n");

        Debug.Log("Follow these steps:\n");
        Debug.Log("1. Go to: Window > TextMeshPro > Font Asset Creator\n");
        Debug.Log("2. Configure these settings:");
        Debug.Log($"   - Font Source: {emojiFont.name}");
        Debug.Log("   - Sampling Point Size: 90");
        Debug.Log("   - Padding: 5");
        Debug.Log("   - Packing Method: Optimum");
        Debug.Log("   - Atlas Resolution: 1024 x 1024");
        Debug.Log("   - Character Set: Custom Characters\n");

        Debug.Log("3. In 'Custom Character List', paste these emojis:");
        Debug.Log($"   {emojiChars}\n");

        Debug.Log("4. Click 'Generate Font Atlas'\n");

        Debug.Log("5. Save as: NotoEmoji SDF");
        Debug.Log("   Location: Assets/TextMesh Pro/Resources/Fonts & Materials/\n");

        Debug.Log("6. Add to TMP Settings:");
        Debug.Log("   - Open: Assets/TextMesh Pro/Resources/TMP Settings.asset");
        Debug.Log("   - Find: 'Fallback Font Assets'");
        Debug.Log("   - Click '+' and add 'NotoEmoji SDF'\n");

        Debug.Log("========================================");
        Debug.Log("ALTERNATIVE: Copy Character List");
        Debug.Log("========================================");
        Debug.Log("If you need Unicode decimal values, use:");
        Debug.Log("Tools > List All Emoji Unicode Values");
        Debug.Log("========================================\n");

        // Copy to clipboard would be nice but requires additional packages
        GUIUtility.systemCopyBuffer = emojiChars;
        Debug.Log("✓ Emoji characters copied to clipboard!");
        Debug.Log($"You can now paste them directly into Font Asset Creator.\n");
    }

    private static void ShowEmojiCharacterList()
    {
        string chars = "🔥🌿💧❌❓★☆❤⚔🛡⚡🌱🎨✓○";
        Debug.Log($"    {chars}");
        Debug.Log($"    Or copy from the custom character section below.");
    }

    [MenuItem("Tools/List All Emoji Unicode Values")]
    public static void ListEmojiUnicodes()
    {
        Debug.Log("=== Emoji Unicode Values for Sketch-Blossom ===");
        Debug.Log("Fire: U+1F525 (decimal: 128293)");
        Debug.Log("Plant: U+1F33F (decimal: 127807)");
        Debug.Log("Water: U+1F4A7 (decimal: 128167)");
        Debug.Log("Cross: U+274C (decimal: 10060)");
        Debug.Log("Question: U+2753 (decimal: 10067)");
        Debug.Log("Star Filled: U+2605 (decimal: 9733)");
        Debug.Log("Star Outline: U+2606 (decimal: 9734)");
        Debug.Log("Heart: U+2764 (decimal: 10084)");
        Debug.Log("Swords: U+2694 (decimal: 9876)");
        Debug.Log("Shield: U+1F6E1 (decimal: 128737) ← THE ERROR EMOJI");
        Debug.Log("Lightning: U+26A1 (decimal: 9889)");
        Debug.Log("Seedling: U+1F331 (decimal: 127793)");
        Debug.Log("Palette: U+1F3A8 (decimal: 127912)");
        Debug.Log("Check: U+2713 (decimal: 10003)");
        Debug.Log("Circle: U+25CB (decimal: 9675)");
        Debug.Log("==============================================");
    }
}
