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
        [Tooltip("Assign 4 RawImage slots in the Inspector – one per move on a plant page (Block + 3 attacks).")]
        public RawImage[] moveShapePreviews;

        [Tooltip("Assign 4 TextMeshProUGUI labels – one below each shape preview to show move name and shape.")]
        public TextMeshProUGUI[] moveShapeLabels;

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

        // Cached original right margin of pageDescription so we can restore it on non-plant pages.
        private float originalDescriptionRightMargin = -1f;

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
                             "Each plant has <color=yellow>4 unique moves</color>:\n" +
                             "• <color=#5599FF>① Block</color> - Defensive stance\n" +
                             "• <color=#FF9955>② Basic Attack</color> - Quick normal hit\n" +
                             "• <color=#FF5555>③ Element Attack</color> - Type-based move\n" +
                             "• <color=#FF3333>④ Signature Move</color> - Powerful finisher\n\n" +
                             "<b>✏️ Drawing Quality Matters!</b>\n" +
                             "Perfect drawings = 1.5× damage\n" +
                             "Poor drawings = 0.5× damage\n\n" +
                             "<b>📖 Each page shows example drawings!</b>\n" +
                             "Match the numbered shapes to learn each move.\n\n" +
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
                description = "<b>✏️ Drawing Shape Tips:</b>\n" +
                             "• <b>Circle:</b> One closed round stroke\n" +
                             "• <b>Square:</b> Closed box with corners\n" +
                             "• <b>Triangle:</b> Closed 3-corner shape\n" +
                             "• <b>Zigzag:</b> Sharp back-and-forth\n" +
                             "• <b>Spiral:</b> Curved inward/outward swirl\n" +
                             "• <b>Star:</b> Lines from center outward\n" +
                             "• <b>Arrow:</b> Line with V-shaped tip\n" +
                             "• <b>Plus/X:</b> Two crossing lines\n\n" +
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

            // Create one page showing all moves for this plant with numbered references
            // matching the shape preview images displayed alongside the text.
            string movesDescription = $"<b>{plantName}</b>\n" +
                                     $"<i>{plantDescription}</i>\n\n";

            // Move index markers so the player can match text to the preview images
            string[] indexMarkers = { "①", "②", "③", "④" };

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
                string shapeName = FormatShapeName(move.drawingShape);
                string marker = i < indexMarkers.Length ? indexMarkers[i] : "•";

                movesDescription += $"<b>{marker} <color={elementColor}>{move.moveName}</color></b> {moveType}\n";
                movesDescription += $"{move.description}\n";
                movesDescription += $"<size=11><color=#999999>{powerText}</color></size>\n";
                movesDescription += $"<size=12>✏️ <b>Draw: {shapeName}</b>  <i>({move.drawingHint})</i></size>\n";

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
        /// Each preview is paired with an optional label showing the move name, shape, and index marker.
        /// Hides all preview images and labels on non-plant pages (welcome, tips, etc.).
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

            // Cache the original right margin on first call so we can restore it.
            if (originalDescriptionRightMargin < 0f && pageDescription != null)
            {
                originalDescriptionRightMargin = pageDescription.margin.z;
            }

            // Adjust the description text area so it doesn't run under the previews.
            if (pageDescription != null)
            {
                Vector4 m = pageDescription.margin;
                // On plant pages, reserve the right ~30% for shape previews.
                // On other pages, restore full width.
                m.z = isPlantPage ? 160f : (originalDescriptionRightMargin >= 0f ? originalDescriptionRightMargin : 0f);
                pageDescription.margin = m;
            }

            // Show or hide the preview container parent (if it exists) so it doesn't
            // absorb clicks or leave dead space on non-plant pages.
            if (moveShapePreviews.Length > 0 && moveShapePreviews[0] != null)
            {
                Transform container = moveShapePreviews[0].transform.parent?.parent;
                if (container != null && container.name == "ShapePreviewContainer")
                    container.gameObject.SetActive(isPlantPage);
            }

            if (!isPlantPage)
            {
                // Hide all preview slots and labels on non-plant pages.
                foreach (var img in moveShapePreviews)
                    if (img != null) img.gameObject.SetActive(false);
                if (moveShapeLabels != null)
                    foreach (var lbl in moveShapeLabels)
                        if (lbl != null) lbl.gameObject.SetActive(false);
                return;
            }

            string[] indexMarkers = { "①", "②", "③", "④" };
            activePreviewTextures = new Texture2D[moveShapePreviews.Length];

            for (int i = 0; i < moveShapePreviews.Length; i++)
            {
                if (moveShapePreviews[i] == null) continue;

                if (i < page.movesOnPage.Length)
                {
                    MoveData move = page.movesOnPage[i];
                    // Use the move's primary color so it reads on the pale background.
                    Color previewColor = move.primaryColor;
                    previewColor.a = 1f;

                    // Generate a preview for the player to reference when drawing.
                    Texture2D tex = MoveShapePreview.GeneratePreview(move.drawingShape, 120, 120, previewColor);
                    activePreviewTextures[i] = tex;
                    moveShapePreviews[i].texture = tex;
                    moveShapePreviews[i].gameObject.SetActive(true);

                    // Update the label beneath the preview with move name and shape.
                    if (moveShapeLabels != null && i < moveShapeLabels.Length && moveShapeLabels[i] != null)
                    {
                        string marker = i < indexMarkers.Length ? indexMarkers[i] : "•";
                        string shapeName = FormatShapeName(move.drawingShape);
                        moveShapeLabels[i].text = $"{marker} {move.moveName}\n<size=10>{shapeName}</size>";
                        moveShapeLabels[i].gameObject.SetActive(true);
                    }
                }
                else
                {
                    moveShapePreviews[i].gameObject.SetActive(false);
                    if (moveShapeLabels != null && i < moveShapeLabels.Length && moveShapeLabels[i] != null)
                        moveShapeLabels[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Convert a DrawingShape enum value into a human-friendly display name.
        /// </summary>
        private static string FormatShapeName(MoveData.DrawingShape shape)
        {
            switch (shape)
            {
                case MoveData.DrawingShape.Circle:          return "Circle";
                case MoveData.DrawingShape.StraightLine:    return "Straight Line";
                case MoveData.DrawingShape.Zigzag:          return "Zigzag";
                case MoveData.DrawingShape.WavyLine:        return "Wavy Line";
                case MoveData.DrawingShape.Plus:             return "Plus (+)";
                case MoveData.DrawingShape.XCross:           return "X Cross";
                case MoveData.DrawingShape.Arrow:            return "Arrow";
                case MoveData.DrawingShape.MultipleCircles:  return "3 Circles";
                case MoveData.DrawingShape.Star:             return "Star";
                case MoveData.DrawingShape.Square:           return "Square";
                case MoveData.DrawingShape.Triangle:         return "Triangle";
                case MoveData.DrawingShape.Checkmark:        return "Checkmark";
                case MoveData.DrawingShape.Spiral:           return "Spiral";
                default:                                     return shape.ToString();
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
