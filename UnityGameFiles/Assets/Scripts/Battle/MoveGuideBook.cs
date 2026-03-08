using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Interactive move guide book that shows all 27 battle moves
    /// Displays unique colors, effects, and drawing hints for each move
    /// Organized by plant type for easy reference during battle
    /// </summary>
    public class MoveGuideBook : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject bookPanel;
        public Button openBookButton;
        public Button closeBookButton;
        public Button nextPageButton;
        public Button previousPageButton;

        [Header("Page Content")]
        public TextMeshProUGUI pageTitle;
        public TextMeshProUGUI pageDescription;
        public Image moveColorDisplay;           // Shows primary/secondary color gradient
        public Image backgroundPanel;            // For page background color
        public TextMeshProUGUI pageNumberText;

        [Header("Shape Previews")]
        [Tooltip("Assign 3 RawImage slots in the Inspector – one per move row on a plant page.")]
        public RawImage[] moveShapePreviews;

        [Header("Animation")]
        public float transitionSpeed = 5f;
        public bool useSlideAnimation = true;

        private int currentPage = 0;
        private MoveGuidePageData[] pages;
        private bool isBookOpen = false;
        private Vector3 closedPosition;
        private Vector3 openPosition;

        // Textures created for the current page – destroyed when we move to another page.
        private Texture2D[] activePreviewTextures;

        [System.Serializable]
        public class MoveGuidePageData
        {
            public string title;
            [TextArea(4, 8)]
            public string description;
            public Color primaryColor;
            public Color secondaryColor;
            public Color pageBackgroundColor;
            /// <summary>Populated for plant pages – one entry per move (Block, Attack1, Attack2).</summary>
            public MoveData[] movesOnPage;
        }

        private void OnDestroy()
        {
            if (activePreviewTextures != null)
                foreach (var t in activePreviewTextures)
                    if (t != null) Object.Destroy(t);
        }

        private void Start()
        {
            Debug.Log("[MoveGuideBook] Starting initialization...");

            InitializePages();
            SetupButtons();

            if (bookPanel != null)
            {
                // Store positions for animation
                RectTransform rectTransform = bookPanel.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    openPosition = rectTransform.anchoredPosition;
                    closedPosition = new Vector3(openPosition.x + 1200f, openPosition.y, openPosition.z);
                    Debug.Log($"[MoveGuideBook] Positions - Open: {openPosition}, Closed: {closedPosition}");
                }

                // Start with book closed
                CloseBook(instant: true);
            }
            else
            {
                Debug.LogError("[MoveGuideBook] bookPanel is NULL! Assign it in Inspector.");
            }

            if (openBookButton != null)
            {
                openBookButton.gameObject.SetActive(true);
                Debug.Log("[MoveGuideBook] Open button is active");
            }
        }

        private void InitializePages()
        {
            List<MoveGuidePageData> pageList = new List<MoveGuidePageData>();

            // ═══════════════════════════════════════════════════════════
            // WELCOME PAGE
            // ═══════════════════════════════════════════════════════════
            pageList.Add(new MoveGuidePageData
            {
                title = "⚔️ Battle Move Guide",
                description = "<b>Master the Art of Combat Drawing!</b>\n\n" +
                             "Each plant has <color=yellow>3 unique moves</color>:\n" +
                             "• <color=#5599FF>Block</color> - Defensive stance (all plants)\n" +
                             "• <color=#FF5555>Attack Move 1</color> - Element-based attack\n" +
                             "• <color=#FF5555>Attack Move 2</color> - Powerful signature move\n\n" +
                             "<b>✏️ Drawing Quality Matters!</b>\n" +
                             "Perfect drawings = 1.5× damage\n" +
                             "Poor drawings = 0.5× damage\n\n" +
                             "<b>🎨 Each move has unique colors!</b>\n" +
                             "Watch for visual effects during battle.\n\n" +
                             "→ Use arrows to explore all moves!",
                primaryColor = new Color(1f, 0.9f, 0.3f),
                secondaryColor = new Color(1f, 0.6f, 0.2f),
                pageBackgroundColor = new Color(0.95f, 0.95f, 0.85f)
            });

            // ═══════════════════════════════════════════════════════════
            // FIRE PLANTS
            // ═══════════════════════════════════════════════════════════

            // SUNFLOWER
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.Sunflower,
                "🔥 Sunflower", "Fire", "Golden solar flames");

            // FIRE ROSE
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.FireRose,
                "🔥 Fire Rose", "Fire", "Crimson burning petals");

            // FLAME TULIP
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.FlameTulip,
                "🔥 Flame Tulip", "Fire", "Intense inferno attacks");

            // ═══════════════════════════════════════════════════════════
            // GRASS PLANTS
            // ═══════════════════════════════════════════════════════════

            // CACTUS
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.Cactus,
                "🌿 Cactus", "Grass", "Sharp desert needles");

            // VINE FLOWER
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.VineFlower,
                "🌿 Vine Flower", "Grass", "Strangling vine attacks");

            // GRASS SPROUT
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.GrassSprout,
                "🌿 Grass Sprout", "Grass", "Rapid growth assaults");

            // ═══════════════════════════════════════════════════════════
            // WATER PLANTS
            // ═══════════════════════════════════════════════════════════

            // WATER LILY
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.WaterLily,
                "💧 Water Lily", "Water", "Tranquil healing waters");

            // CORAL BLOOM
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.CoralBloom,
                "💧 Coral Bloom", "Water", "Sharp coral strikes");

            // BUBBLE FLOWER
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.BubbleFlower,
                "💧 Bubble Flower", "Water", "Healing bubble magic");

            // ═══════════════════════════════════════════════════════════
            // TIPS & TRICKS PAGE
            // ═══════════════════════════════════════════════════════════
            pageList.Add(new MoveGuidePageData
            {
                title = "⚡ Combat Master Tips",
                description = "<b>✏️ Drawing Patterns:</b>\n" +
                             "• <b>Block:</b> 1-3 simple circles\n" +
                             "• <b>Fireball/Bubble:</b> Perfect circles\n" +
                             "• <b>Burn:</b> Sharp zigzags (lightning)\n" +
                             "• <b>VineWhip:</b> Single smooth curve\n" +
                             "• <b>LeafStorm:</b> 5+ scattered strokes\n" +
                             "• <b>RootAttack:</b> Vertical lines downward\n" +
                             "• <b>WaterSplash:</b> Wavy flowing curves\n" +
                             "• <b>HealingWave:</b> Gentle horizontal waves\n\n" +
                             "<b>⚔️ Type Advantages:</b>\n" +
                             "💧 Water > 🔥 Fire (1.5× damage)\n" +
                             "🔥 Fire > 🌿 Grass (1.5× damage)\n" +
                             "🌿 Grass > 💧 Water (1.5× damage)\n\n" +
                             "<b>Practice makes perfect! ✨</b>",
                primaryColor = new Color(0.5f, 0.9f, 1f),
                secondaryColor = new Color(1f, 0.5f, 0.9f),
                pageBackgroundColor = new Color(0.95f, 0.95f, 1f)
            });

            pages = pageList.ToArray();
            Debug.Log($"[MoveGuideBook] Initialized {pages.Length} pages");
            UpdatePageDisplay();
        }

        /// <summary>
        /// Add a page for each plant's moves
        /// </summary>
        private void AddPlantMovesPages(List<MoveGuidePageData> pageList,
            PlantRecognitionSystem.PlantType plantType,
            string plantName, string elementName, string plantDescription)
        {
            MoveData[] moves = MoveData.GetMovesForPlant(plantType);
            if (moves == null || moves.Length == 0)
            {
                Debug.LogWarning($"[MoveGuideBook] No moves found for {plantType}");
                return;
            }

            // Create one page showing all 3 moves for this plant
            string movesDescription = $"<b>{plantName}</b>\n" +
                                     $"<i>{plantDescription}</i>\n\n";

            for (int i = 0; i < moves.Length; i++)
            {
                MoveData move = moves[i];

                // Get element color for text
                string elementColor = elementName == "Fire" ? "#FF6633" :
                                    elementName == "Grass" ? "#66DD66" :
                                    "#5599FF"; // Water

                // Build move description
                string moveType = move.isDefensiveMove ? "🛡️ Defense" :
                                move.isHealingMove ? "💚 Healing" :
                                "⚔️ Attack";

                string powerText = move.basePower > 0 ? $"PWR: {move.basePower}" : "Reduces damage";

                movesDescription += $"<b><color={elementColor}>{move.moveName}</color></b> {moveType}\n";
                movesDescription += $"{move.description}\n";
                movesDescription += $"<size=11><color=#999999>{powerText}</color></size>\n";
                movesDescription += $"<size=11>✏️ <i>{move.drawingHint}</i></size>\n";

                if (i < moves.Length - 1)
                    movesDescription += "\n";
            }

            // Use the plant's first attack move colors for the page theme
            // (Skip Block which is always first)
            MoveData themeMove = moves.Length > 1 ? moves[1] : moves[0];

            pageList.Add(new MoveGuidePageData
            {
                title = plantName,
                description = movesDescription,
                primaryColor = themeMove.primaryColor,
                secondaryColor = themeMove.secondaryColor,
                pageBackgroundColor = GetPageBackgroundColor(elementName),
                movesOnPage = moves
            });
        }

        private Color GetPageBackgroundColor(string elementName)
        {
            switch (elementName)
            {
                case "Fire":
                    return new Color(1f, 0.92f, 0.8f);  // Warm cream
                case "Grass":
                    return new Color(0.85f, 0.98f, 0.85f);  // Light green
                case "Water":
                    return new Color(0.85f, 0.93f, 1f);  // Light blue
                default:
                    return new Color(0.95f, 0.95f, 0.95f);  // Light gray
            }
        }

        private void SetupButtons()
        {
            if (openBookButton != null)
            {
                openBookButton.onClick.RemoveAllListeners();
                openBookButton.onClick.AddListener(() => OpenBook());
                Debug.Log("[MoveGuideBook] Open button listener added");
            }

            if (closeBookButton != null)
            {
                closeBookButton.onClick.RemoveAllListeners();
                closeBookButton.onClick.AddListener(() => CloseBook());
                Debug.Log("[MoveGuideBook] Close button listener added");
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick.RemoveAllListeners();
                nextPageButton.onClick.AddListener(NextPage);
            }

            if (previousPageButton != null)
            {
                previousPageButton.onClick.RemoveAllListeners();
                previousPageButton.onClick.AddListener(PreviousPage);
            }
        }

        public void OpenBook()
        {
            Debug.Log("[MoveGuideBook] OpenBook() called");

            if (isBookOpen)
            {
                Debug.Log("[MoveGuideBook] Book already open");
                return;
            }

            isBookOpen = true;

            if (bookPanel != null)
            {
                bookPanel.SetActive(true);
                bookPanel.transform.SetAsLastSibling(); // Bring to front

                if (useSlideAnimation)
                {
                    StartCoroutine(AnimateBookPosition(openPosition));
                }
                else
                {
                    RectTransform rect = bookPanel.GetComponent<RectTransform>();
                    if (rect != null) rect.anchoredPosition = openPosition;
                }

                Debug.Log("[MoveGuideBook] Book opened successfully");
            }

            if (openBookButton != null)
            {
                openBookButton.gameObject.SetActive(false);
            }
        }

        public void CloseBook(bool instant = false)
        {
            if (!isBookOpen && !instant) return;

            isBookOpen = false;

            if (bookPanel != null)
            {
                if (useSlideAnimation && !instant)
                {
                    StartCoroutine(AnimateBookPosition(closedPosition, () =>
                    {
                        bookPanel.SetActive(false);
                    }));
                }
                else
                {
                    RectTransform rectTransform = bookPanel.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = closedPosition;
                    }
                    bookPanel.SetActive(false);
                }
            }

            if (openBookButton != null)
            {
                openBookButton.gameObject.SetActive(true);
            }

            Debug.Log("[MoveGuideBook] Book closed");
        }

        public void NextPage()
        {
            if (currentPage < pages.Length - 1)
            {
                currentPage++;
                UpdatePageDisplay();
                Debug.Log($"[MoveGuideBook] Next page: {currentPage + 1}/{pages.Length}");
            }
        }

        public void PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdatePageDisplay();
                Debug.Log($"[MoveGuideBook] Previous page: {currentPage + 1}/{pages.Length}");
            }
        }

        private void UpdatePageDisplay()
        {
            if (pages == null || pages.Length == 0 || currentPage >= pages.Length)
                return;

            MoveGuidePageData page = pages[currentPage];

            // Update title
            if (pageTitle != null)
            {
                pageTitle.text = page.title;
            }

            // Update description
            if (pageDescription != null)
            {
                pageDescription.text = page.description;
            }

            // Update page number
            if (pageNumberText != null)
            {
                pageNumberText.text = $"Page {currentPage + 1} / {pages.Length}";
            }

            // Update color display with gradient
            if (moveColorDisplay != null)
            {
                // Create a simple vertical gradient texture
                Texture2D gradientTexture = CreateGradientTexture(page.primaryColor, page.secondaryColor);
                Sprite gradientSprite = Sprite.Create(gradientTexture,
                    new Rect(0, 0, gradientTexture.width, gradientTexture.height),
                    new Vector2(0.5f, 0.5f));
                moveColorDisplay.sprite = gradientSprite;
                moveColorDisplay.color = Color.white;
            }

            // Update background color
            if (backgroundPanel != null)
            {
                backgroundPanel.color = page.pageBackgroundColor;
            }
            else if (bookPanel != null)
            {
                Image panelImage = bookPanel.GetComponent<Image>();
                if (panelImage != null)
                {
                    panelImage.color = page.pageBackgroundColor;
                }
            }

            // Update button states
            if (previousPageButton != null)
            {
                previousPageButton.interactable = currentPage > 0;
            }

            if (nextPageButton != null)
            {
                nextPageButton.interactable = currentPage < pages.Length - 1;
            }

            // Update shape reference previews
            UpdateShapePreviews(page);
        }

        /// <summary>
        /// Generate and display the reference-drawing shape for each move on the current plant page.
        /// Hides all preview images on non-plant pages (welcome, tips, etc.).
        /// </summary>
        private void UpdateShapePreviews(MoveGuidePageData page)
        {
            if (moveShapePreviews == null || moveShapePreviews.Length == 0)
                return;

            // Destroy textures we created for the previous page to avoid leaks.
            if (activePreviewTextures != null)
            {
                foreach (var t in activePreviewTextures)
                    if (t != null) Object.Destroy(t);
            }
            activePreviewTextures = null;

            bool isPlantPage = page.movesOnPage != null && page.movesOnPage.Length > 0;

            if (!isPlantPage)
            {
                // Hide all preview slots on non-plant pages.
                foreach (var img in moveShapePreviews)
                    if (img != null) img.gameObject.SetActive(false);
                return;
            }

            activePreviewTextures = new Texture2D[moveShapePreviews.Length];

            for (int i = 0; i < moveShapePreviews.Length; i++)
            {
                if (moveShapePreviews[i] == null) continue;

                if (i < page.movesOnPage.Length)
                {
                    MoveData move = page.movesOnPage[i];
                    // Use a darkened version of the move's primary color so it reads on the pale background.
                    Color previewColor = move.primaryColor;
                    previewColor.a = 1f;

                    Texture2D tex = MoveShapePreview.GeneratePreview(move.moveType, 80, 80, previewColor);
                    activePreviewTextures[i] = tex;
                    moveShapePreviews[i].texture = tex;
                    moveShapePreviews[i].gameObject.SetActive(true);
                }
                else
                {
                    moveShapePreviews[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Create a simple vertical gradient texture from two colors
        /// </summary>
        private Texture2D CreateGradientTexture(Color topColor, Color bottomColor)
        {
            int width = 32;
            int height = 128;
            Texture2D texture = new Texture2D(width, height);

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height;
                Color color = Color.Lerp(bottomColor, topColor, t);

                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        private System.Collections.IEnumerator AnimateBookPosition(Vector3 targetPosition, System.Action onComplete = null)
        {
            if (bookPanel == null) yield break;

            RectTransform rectTransform = bookPanel.GetComponent<RectTransform>();
            if (rectTransform == null) yield break;

            Vector3 startPosition = rectTransform.anchoredPosition;
            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            rectTransform.anchoredPosition = targetPosition;
            onComplete?.Invoke();
        }

        public void GoToPage(int pageIndex)
        {
            if (pageIndex >= 0 && pageIndex < pages.Length)
            {
                currentPage = pageIndex;
                UpdatePageDisplay();
            }
        }

        // Keyboard shortcuts
        private void Update()
        {
            // Press M to toggle move guide book
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (isBookOpen)
                {
                    CloseBook();
                }
                else
                {
                    OpenBook();
                }
            }

            if (isBookOpen)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    NextPage();
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    PreviousPage();
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseBook();
                }
            }
        }

        /// <summary>
        /// Get total number of pages
        /// </summary>
        public int GetPageCount()
        {
            return pages != null ? pages.Length : 0;
        }

        /// <summary>
        /// Check if book is open
        /// </summary>
        public bool IsOpen()
        {
            return isBookOpen;
        }
    }
}
