using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Helper to download and setup free UI assets for Sketch Blossom
/// </summary>
public class AssetDownloader : EditorWindow
{
    private Vector2 scrollPosition;
    private bool downloadInProgress = false;

    [MenuItem("Tools/Sketch Blossom/Download Free Assets")]
    public static void ShowWindow()
    {
        AssetDownloader window = GetWindow<AssetDownloader>("Asset Downloader");
        window.minSize = new Vector2(500, 600);
        window.Show();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Free Asset Downloader", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool provides links to free assets for Sketch Blossom.\n" +
            "Click the buttons below to open the download pages in your browser.",
            MessageType.Info);

        GUILayout.Space(20);

        // Kenney Assets
        DrawSection("Kenney.nl - UI Packs (Recommended)", new[]
        {
            new AssetLink("UI Pack", "https://kenney.nl/assets/ui-pack", "Complete UI sprite pack with buttons, panels, and icons"),
            new AssetLink("UI Pack Space", "https://kenney.nl/assets/ui-pack-space-expansion", "Space-themed UI elements"),
            new AssetLink("Game Icons", "https://kenney.nl/assets/game-icons", "Various game icons including nature themes"),
            new AssetLink("Pixel UI Pack", "https://kenney.nl/assets/pixel-ui-pack", "Retro pixel art UI elements")
        });

        GUILayout.Space(10);

        // Unity Asset Store
        DrawSection("Unity Asset Store - Free Section", new[]
        {
            new AssetLink("Simple Button Set", "https://assetstore.unity.com/packages/2d/gui/icons/simple-button-set-114016", "Clean, modern button set"),
            new AssetLink("Casual Game UI", "https://assetstore.unity.com/packages/2d/gui/casual-game-ui-224249", "Friendly casual game UI"),
            new AssetLink("UI Samples", "https://assetstore.unity.com/packages/essentials/ui-samples-25468", "Official Unity UI samples")
        });

        GUILayout.Space(10);

        // Fonts
        DrawSection("Google Fonts (Free)", new[]
        {
            new AssetLink("Fredoka One", "https://fonts.google.com/specimen/Fredoka+One", "Playful, rounded font"),
            new AssetLink("Nunito", "https://fonts.google.com/specimen/Nunito", "Friendly, readable font"),
            new AssetLink("Quicksand", "https://fonts.google.com/specimen/Quicksand", "Modern, clean font"),
            new AssetLink("Comic Neue", "https://fonts.google.com/specimen/Comic+Neue", "Comic-style improved font")
        });

        GUILayout.Space(10);

        // Icons
        DrawSection("Icon Resources", new[]
        {
            new AssetLink("Font Awesome", "https://fontawesome.com/search?o=r&m=free", "Huge collection of free icons"),
            new AssetLink("Icons8", "https://icons8.com/icons/set/book", "Free icons (attribution required)"),
            new AssetLink("Flaticon", "https://www.flaticon.com/search?word=plant", "Plant and nature icons")
        });

        GUILayout.Space(10);

        // Textures
        DrawSection("Background Textures", new[]
        {
            new AssetLink("OpenGameArt - UI", "https://opengameart.org/art-search-advanced?keys=ui&field_art_type_tid%5B%5D=9", "Free game UI textures"),
            new AssetLink("Subtle Patterns", "https://www.toptal.com/designers/subtlepatterns/", "Subtle background patterns"),
            new AssetLink("Textures.com (Free)", "https://www.textures.com/browse/paper/111793", "Paper and parchment textures")
        });

        GUILayout.Space(20);

        // Import instructions
        EditorGUILayout.HelpBox(
            "IMPORT INSTRUCTIONS:\n\n" +
            "1. Download assets from the links above\n" +
            "2. For PNG/JPG: Drag into Unity's Assets folder\n" +
            "3. For Fonts: Import .ttf/.otf, then create TextMeshPro Font Asset\n" +
            "4. For Unity Packages: Double-click to import\n" +
            "5. Apply sprites to UI elements in Inspector",
            MessageType.Info);

        GUILayout.Space(10);

        // Quick setup button
        if (GUILayout.Button("Open Quick Asset Import Folder", GUILayout.Height(30)))
        {
            CreateAssetFolder();
        }

        EditorGUILayout.HelpBox(
            "This creates an 'ImportedAssets' folder in your project.\n" +
            "Download assets and drag them into this folder!",
            MessageType.Info);

        GUILayout.Space(20);

        // Auto-apply button
        EditorGUILayout.LabelField("Auto-Setup (Experimental)", EditorStyles.boldLabel);

        if (GUILayout.Button("Apply Default Unity UI Sprites", GUILayout.Height(30)))
        {
            ApplyDefaultSprites();
        }

        EditorGUILayout.HelpBox(
            "This applies Unity's built-in UI sprites to your Drawing Scene.\n" +
            "Better than nothing while you download custom assets!",
            MessageType.Info);

        GUILayout.Space(10);

        EditorGUILayout.EndScrollView();
    }

    private void DrawSection(string title, AssetLink[] links)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        foreach (var link in links)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(link.name, GUILayout.Width(150));
            EditorGUILayout.LabelField(link.description, EditorStyles.wordWrappedLabel);

            if (GUILayout.Button("Open", GUILayout.Width(60)))
            {
                Application.OpenURL(link.url);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
    }

    private void CreateAssetFolder()
    {
        string folderPath = "Assets/ImportedAssets";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "ImportedAssets");

            // Create subfolders
            AssetDatabase.CreateFolder(folderPath, "UI");
            AssetDatabase.CreateFolder(folderPath, "Fonts");
            AssetDatabase.CreateFolder(folderPath, "Icons");
            AssetDatabase.CreateFolder(folderPath, "Backgrounds");

            AssetDatabase.Refresh();

            Debug.Log($"Created ImportedAssets folder at: {folderPath}");
            EditorUtility.DisplayDialog(
                "Folder Created!",
                $"Created folder structure:\n\n" +
                $"{folderPath}/\n" +
                $"  ├── UI/\n" +
                $"  ├── Fonts/\n" +
                $"  ├── Icons/\n" +
                $"  └── Backgrounds/\n\n" +
                $"Download assets and drag them into these folders!",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Folder Exists", "ImportedAssets folder already exists!", "OK");
        }

        // Ping the folder in Project window
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
        EditorGUIUtility.PingObject(obj);
        Selection.activeObject = obj;
    }

    private void ApplyDefaultSprites()
    {
        // Use Unity's built-in UI sprites
        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Sprite checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");

        int count = 0;

        // Find all buttons in scene
        Button[] buttons = GameObject.FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null && buttonImage.sprite == null)
            {
                buttonImage.sprite = uiSprite;
                buttonImage.type = Image.Type.Sliced;
                EditorUtility.SetDirty(buttonImage);
                count++;
            }
        }

        // Find all panels
        Image[] images = GameObject.FindObjectsOfType<Image>();
        foreach (Image image in images)
        {
            if (image.sprite == null && image.gameObject.name.Contains("Panel"))
            {
                image.sprite = background;
                image.type = Image.Type.Sliced;
                EditorUtility.SetDirty(image);
                count++;
            }
        }

        if (count > 0)
        {
            Debug.Log($"Applied default sprites to {count} UI elements");
            EditorUtility.DisplayDialog(
                "Sprites Applied!",
                $"Applied Unity's built-in UI sprites to {count} elements.\n\n" +
                "Now download custom assets for better visuals!",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog(
                "No Changes",
                "All UI elements already have sprites assigned, or no UI elements found.",
                "OK");
        }
    }

    private struct AssetLink
    {
        public string name;
        public string url;
        public string description;

        public AssetLink(string name, string url, string description)
        {
            this.name = name;
            this.url = url;
            this.description = description;
        }
    }
}
