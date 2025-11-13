using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// Applies Unity UI Samples assets to make Drawing Scene look beautiful
/// </summary>
public class UIAssetApplier : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Apply UI Assets to Scene", priority = 2)]
    public static void ApplyAssets()
    {
        Debug.Log("=== APPLYING UI ASSETS ===");

        int applied = 0;

        // Load sprites from Unity UI Samples
        Sprite buttonSprite = LoadSprite("Assets/Unity UI Samples/Textures and Sprites/Rounded UI/UIButtonDefault.png");
        Sprite panelSprite = LoadSprite("Assets/Unity UI Samples/Textures and Sprites/Rounded UI/UIPanel.png");
        Sprite backgroundSprite = LoadSprite("Assets/Unity UI Samples/Textures and Sprites/SF UI/Background/SF Background.png");

        // Apply to buttons
        applied += ApplyToButtons(buttonSprite);

        // Apply to panels
        applied += ApplyToPanels(panelSprite, backgroundSprite);

        // Update colors for better visuals
        applied += UpdateButtonColors();

        Debug.Log($"=== APPLIED ASSETS TO {applied} ELEMENTS ===");

        EditorUtility.DisplayDialog(
            "Assets Applied!",
            $"Applied UI assets to {applied} elements!\n\n" +
            "✅ Buttons styled with rounded sprites\n" +
            "✅ Panels styled with backgrounds\n" +
            "✅ Colors updated for better visuals\n\n" +
            "Your Drawing Scene now looks much better!",
            "Awesome!"
        );
    }

    private static Sprite LoadSprite(string path)
    {
        // Try to load the sprite
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (sprite == null)
        {
            // Try alternate path without extension
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path.Replace(".png", ""));
        }

        if (sprite != null)
        {
            Debug.Log($"✅ Loaded sprite: {path}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Could not load sprite: {path}");
        }

        return sprite;
    }

    private static int ApplyToButtons(Sprite buttonSprite)
    {
        if (buttonSprite == null)
        {
            Debug.LogWarning("Button sprite not found, skipping button styling");
            return 0;
        }

        int count = 0;
        Button[] buttons = Object.FindObjectsOfType<Button>();

        foreach (var button in buttons)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = buttonSprite;
                buttonImage.type = Image.Type.Sliced;

                // Set nice colors based on button type
                if (button.gameObject.name.Contains("Start"))
                {
                    // Green for start button
                    buttonImage.color = new Color(0.3f, 0.85f, 0.4f);
                }
                else if (button.gameObject.name.Contains("Guide"))
                {
                    // Blue for guide button
                    buttonImage.color = new Color(0.3f, 0.7f, 1f);
                }
                else if (button.gameObject.name.Contains("Finish"))
                {
                    // Orange for finish button
                    buttonImage.color = new Color(1f, 0.6f, 0.2f);
                }
                else
                {
                    // Default nice blue
                    buttonImage.color = new Color(0.4f, 0.7f, 0.95f);
                }

                EditorUtility.SetDirty(buttonImage);
                Debug.Log($"✅ Applied sprite to button: {button.gameObject.name}");
                count++;
            }
        }

        return count;
    }

    private static int ApplyToPanels(Sprite panelSprite, Sprite backgroundSprite)
    {
        int count = 0;

        // Find all panel objects
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("Panel"))
            {
                Image panelImage = obj.GetComponent<Image>();
                if (panelImage != null)
                {
                    // Use different sprites for different panels
                    if (obj.name == "InstructionsPanel" && backgroundSprite != null)
                    {
                        panelImage.sprite = backgroundSprite;
                        panelImage.color = new Color(1f, 1f, 1f, 0.95f);
                    }
                    else if (panelSprite != null)
                    {
                        panelImage.sprite = panelSprite;
                        panelImage.type = Image.Type.Sliced;

                        // Nice soft colors
                        if (obj.name.Contains("Drawing"))
                        {
                            panelImage.color = new Color(0.95f, 0.98f, 0.95f, 1f);
                        }
                        else if (obj.name.Contains("GuideBook"))
                        {
                            panelImage.color = new Color(0.98f, 0.96f, 0.88f, 1f);
                        }
                        else
                        {
                            panelImage.color = new Color(1f, 1f, 1f, 0.9f);
                        }
                    }

                    EditorUtility.SetDirty(panelImage);
                    Debug.Log($"✅ Applied sprite to panel: {obj.name}");
                    count++;
                }
            }
        }

        return count;
    }

    private static int UpdateButtonColors()
    {
        int count = 0;
        Button[] buttons = Object.FindObjectsOfType<Button>();

        foreach (var button in buttons)
        {
            ColorBlock colors = button.colors;

            // Nice color transitions
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            // Higher contrast
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;

            button.colors = colors;
            EditorUtility.SetDirty(button);
            count++;
        }

        Debug.Log($"✅ Updated colors for {count} buttons");
        return count;
    }

    [MenuItem("Tools/Sketch Blossom/Style Drawing Area", priority = 3)]
    public static void StyleDrawingArea()
    {
        GameObject drawingArea = GameObject.Find("DrawingArea");

        if (drawingArea != null)
        {
            Image image = drawingArea.GetComponent<Image>();
            if (image == null)
            {
                image = drawingArea.AddComponent<Image>();
            }

            // Nice clean white with subtle border
            image.color = Color.white;

            // Add outline component for border
            Outline outline = drawingArea.GetComponent<Outline>();
            if (outline == null)
            {
                outline = drawingArea.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0.7f, 0.7f, 0.7f);
            outline.effectDistance = new Vector2(2, -2);

            EditorUtility.SetDirty(drawingArea);

            Debug.Log("✅ Styled Drawing Area");
            EditorUtility.DisplayDialog("Success", "Drawing Area styled with clean white background and border!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Not Found", "DrawingArea not found in scene", "OK");
        }
    }

    [MenuItem("Tools/Sketch Blossom/Add Background Gradient", priority = 4)]
    public static void AddBackgroundGradient()
    {
        // Find Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Check if background already exists
        Transform existingBg = canvas.transform.Find("Background");
        if (existingBg != null)
        {
            Object.DestroyImmediate(existingBg.gameObject);
        }

        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(canvas.transform, false);
        background.transform.SetAsFirstSibling(); // Put it behind everything

        RectTransform rect = background.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = background.AddComponent<Image>();

        // Try to load background sprite
        Sprite bgSprite = LoadSprite("Assets/Unity UI Samples/Textures and Sprites/SF UI/Background/SF Background.png");

        if (bgSprite != null)
        {
            image.sprite = bgSprite;
            image.color = new Color(0.9f, 0.95f, 0.92f); // Light green tint
        }
        else
        {
            // Gradient color
            image.color = new Color(0.85f, 0.92f, 0.88f);
        }

        Debug.Log("✅ Added background");
        EditorUtility.DisplayDialog("Success", "Background added to Drawing Scene!", "OK");
    }
}
