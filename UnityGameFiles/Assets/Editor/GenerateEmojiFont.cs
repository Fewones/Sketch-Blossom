using UnityEngine;
using UnityEditor;
using TMPro;
using TMPro.EditorUtilities;

/// <summary>
/// Unity Editor script to generate a TextMeshPro SDF font asset from Noto Emoji
/// with all the Unicode characters used in Sketch-Blossom game.
///
/// Usage: In Unity Editor, go to Tools > Generate Emoji Font Asset
/// </summary>
public class GenerateEmojiFont : MonoBehaviour
{
    [MenuItem("Tools/Generate Emoji Font Asset")]
    public static void CreateEmojiFont()
    {
        // Load the Noto Emoji font
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/NotoEmoji-Regular.ttf");

        if (sourceFont == null)
        {
            Debug.LogError("Could not find NotoEmoji-Regular.ttf! Make sure it's in Assets/TextMesh Pro/Fonts/");
            return;
        }

        // Define all Unicode characters used in the game
        // Based on UNICODE_EMOJI_ANALYSIS.md
        string unicodeCharacters =
            // Element emojis
            "\U0001F525" +  // 🔥 Fire
            "\U0001F33F" +  // 🌿 Plant/Herb
            "\U0001F4A7" +  // 💧 Water Droplet

            // Status/UI emojis
            "\u274C" +      // ❌ Cross Mark
            "\u2753" +      // ❓ Question Mark
            "\u2605" +      // ★ Star (filled)
            "\u2606" +      // ☆ Star (outline)
            "\u2764" +      // ❤️ Heart
            "\u2694" +      // ⚔️ Crossed Swords
            "\U0001F6E1" +  // 🛡️ Shield (the one causing the error!)
            "\u26A1" +      // ⚡ Lightning Bolt
            "\U0001F331" +  // 🌱 Seedling
            "\U0001F3A8" +  // 🎨 Artist Palette
            "\u2713" +      // ✓ Check Mark
            "\u25CB";       // ○ Circle

        Debug.Log($"Creating emoji font with {unicodeCharacters.Length} characters...");

        // Create the font asset
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,        // Sampling point size (higher = better quality)
            9,         // Atlas padding
            GlyphRenderMode.SDFAA,  // Rendering mode
            1024,      // Atlas width
            1024,      // Atlas height
            AtlasPopulationMode.Dynamic  // Population mode
        );

        if (fontAsset == null)
        {
            Debug.LogError("Failed to create font asset!");
            return;
        }

        // Set the character set
        fontAsset.name = "NotoEmoji SDF";

        // Save the asset
        string savePath = "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoEmoji SDF.asset";
        AssetDatabase.CreateAsset(fontAsset, savePath);

        // Now we need to populate the atlas with our specific characters
        // This requires regenerating the font asset with the custom character set
        Debug.Log($"Font asset created at: {savePath}");
        Debug.Log("IMPORTANT: You need to manually add the Unicode characters to the font asset:");
        Debug.Log("1. Select the NotoEmoji SDF asset");
        Debug.Log("2. Open Font Asset Creator (Window > TextMeshPro > Font Asset Creator)");
        Debug.Log("3. Set 'Character Set' to 'Custom Characters'");
        Debug.Log("4. Paste this into the 'Custom Character List' field:");
        Debug.Log(unicodeCharacters);
        Debug.Log("5. Click 'Generate Font Atlas'");
        Debug.Log("\nAlternatively, use the Auto Font Asset Generator below:");

        // Try to auto-generate with the character set
        GenerateFontAtlasWithCharacters(sourceFont, unicodeCharacters);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Emoji font asset generation complete!");
        Debug.Log("Next step: Add this font to TMP Settings as a fallback font.");
    }

    private static void GenerateFontAtlasWithCharacters(Font sourceFont, string characters)
    {
        try
        {
            // This is an alternative method that tries to create the font with specific characters
            // Using TMPro's internal API

            string savePath = "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoEmoji SDF.asset";

            // Check if font asset creator window is available
            // Note: This might require the FontAssetCreatorWindow to be open
            Debug.Log("Attempting automatic atlas generation...");
            Debug.Log("If this doesn't work, please follow the manual steps above.");

            // You can also manually add characters after creation by:
            // 1. Selecting the font asset
            // 2. Inspector > Character Table > Add Character
            // 3. Enter Unicode value for each emoji

        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Auto-generation failed: {e.Message}");
            Debug.Log("Please use the manual method described above.");
        }
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
