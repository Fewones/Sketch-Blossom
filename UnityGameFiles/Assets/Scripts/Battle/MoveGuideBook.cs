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

        [Header("Move Entry Rows (Plant Pages)")]
        [Tooltip("Container holding the individual move rows. Hidden on non-plant pages.")]
        public Transform moveEntriesContainer;

        [Tooltip("Assign 4 TextMeshProUGUI elements – one per move row for the move text.")]
        public TextMeshProUGUI[] moveEntryTexts;

        [Tooltip("Assign 4 RawImage slots – one per move row for the shape preview.")]
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

        // Cached original anchors of pageDescription so we can restore on non-plant pages.
        private Vector2 origDescAnchorMin;
        private Vector2 origDescAnchorMax;
        private bool cachedDescAnchors = false;

        [System.Serializable]
        public class MoveGuidePageData
        {
            public string title;
            [TextArea(4, 8)]
            public string description;
            public Color primaryColor;
            public Color secondaryColor;
            public Color pageBackgroundColor;
            /// <summary>Populated for plant pages – one entry per move (Block, Attack1, Attack2, Attack3).</summary>
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

            // Ensure the move entries container's VerticalLayoutGroup controls
            // child sizing. Without this the rows stay at 0 height and are invisible.
            if (moveEntriesContainer != null)
            {
                var vLayout = moveEntriesContainer.GetComponent<VerticalLayoutGroup>();
                if (vLayout != null)
                {
                    vLayout.childControlWidth = true;
                    vLayout.childControlHeight = true;
                }
            }

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
                title = "Battle Move Guide",
                description = "<b>Master the Art of Combat Drawing!</b>\n\n" +
                             "Each plant has <color=yellow>4 unique moves</color>:\n" +
                             "* <color=#5599FF>Block</color> - Defensive stance\n" +
                             "* <color=#FF9955>Basic Attack</color> - Quick normal hit\n" +
                             "* <color=#FF5555>Element Attack</color> - Type-based move\n" +
                             "* <color=#FF3333>Signature Move</color> - Powerful finisher\n\n" +
                             "<b>Drawing Quality Matters!</b>\n" +
                             "Perfect drawings = 1.5x damage\n" +
                             "Poor drawings = 0.5x damage\n\n" +
                             "<b>Each page shows example drawings!</b>\n" +
                             "Match the shape next to each move to learn it.\n\n" +
                             "Use arrows to explore all moves!",
                primaryColor = new Color(1f, 0.9f, 0.3f),
                secondaryColor = new Color(1f, 0.6f, 0.2f),
                pageBackgroundColor = new Color(0.95f, 0.95f, 0.85f)
            });

            // ═══════════════════════════════════════════════════════════
            // FIRE PLANTS
            // ═══════════════════════════════════════════════════════════
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.Sunflower,
                "Sunflower", "Fire", "Golden solar flames");

            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.FireRose,
                "Fire Rose", "Fire", "Crimson burning petals");

            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.FlameTulip,
                "Flame Tulip", "Fire", "Intense inferno attacks");

            // ═══════════════════════════════════════════════════════════
            // GRASS PLANTS
            // ═══════════════════════════════════════════════════════════
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.Cactus,
                "Cactus", "Grass", "Sharp desert needles");

            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.VineFlower,
                "Vine Flower", "Grass", "Strangling vine attacks");

            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.GrassSprout,
                "Grass Sprout", "Grass", "Rapid growth assaults");

            // ═══════════════════════════════════════════════════════════
            // WATER PLANTS
            // ═══════════════════════════════════════════════════════════
            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.WaterLily,
                "Water Lily", "Water", "Tranquil healing waters");

            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.CoralBloom,
                "Coral Bloom", "Water", "Sharp coral strikes");

            AddPlantMovesPages(pageList, PlantRecognitionSystem.PlantType.BubbleFlower,
                "Bubble Flower", "Water", "Healing bubble magic");

            // ═══════════════════════════════════════════════════════════
            // TIPS & TRICKS PAGE
            // ═══════════════════════════════════════════════════════════
            pageList.Add(new MoveGuidePageData
            {
                title = "Combat Master Tips",
                description = "<b>Drawing Shape Tips:</b>\n" +
                             "* <b>Circle:</b> One closed round stroke\n" +
                             "* <b>Square:</b> Closed box with corners\n" +
                             "* <b>Triangle:</b> Closed 3-corner shape\n" +
                             "* <b>Zigzag:</b> Sharp back-and-forth\n" +
                             "* <b>Spiral:</b> Curved inward/outward swirl\n" +
                             "* <b>Star:</b> Lines from center outward\n" +
                             "* <b>Arrow:</b> Line with V-shaped tip\n" +
                             "* <b>Plus/X:</b> Two crossing lines\n\n" +
                             "<b>Type Advantages:</b>\n" +
                             "Water > Fire (1.5x damage)\n" +
                             "Fire > Grass (1.5x damage)\n" +
                             "Grass > Water (1.5x damage)\n\n" +
                             "<b>Practice makes perfect!</b>",
                primaryColor = new Color(0.5f, 0.9f, 1f),
                secondaryColor = new Color(1f, 0.5f, 0.9f),
                pageBackgroundColor = new Color(0.95f, 0.95f, 1f)
            });

            pages = pageList.ToArray();
            Debug.Log($"[MoveGuideBook] Initialized {pages.Length} pages");
            UpdatePageDisplay();
        }

        /// <summary>
        /// Add a page for each plant's moves.
        /// The description only contains the header; individual moves are displayed
        /// via the moveEntryTexts + moveShapePreviews rows.
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

            // Header-only description; the per-move details are rendered in individual rows.
            string headerDescription = $"<i>{plantDescription}</i>";

            // Use the plant's first attack move colors for the page theme
            MoveData themeMove = moves.Length > 1 ? moves[1] : moves[0];

            pageList.Add(new MoveGuidePageData
            {
                title = plantName,
                description = headerDescription,
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

            // Update move entry rows (plant pages) or hide them (non-plant pages)
            UpdateMoveEntries(page);
        }

        /// <summary>
        /// On plant pages, populate each move row with text on the left and
        /// the shape preview on the right. On non-plant pages, hide the rows.
        /// Also resizes pageDescription: on plant pages it shrinks to just
        /// the header area; on non-plant pages it fills the full space.
        /// </summary>
        private void UpdateMoveEntries(MoveGuidePageData page)
        {
            // Destroy textures from the previous page to avoid leaks.
            if (activePreviewTextures != null)
            {
                foreach (var t in activePreviewTextures)
                    if (t != null) Object.Destroy(t);
            }
            activePreviewTextures = null;

            bool isPlantPage = page.movesOnPage != null && page.movesOnPage.Length > 0;

            // Cache the original pageDescription anchors once so we can restore them.
            if (!cachedDescAnchors && pageDescription != null)
            {
                RectTransform descRect = pageDescription.GetComponent<RectTransform>();
                if (descRect != null)
                {
                    origDescAnchorMin = descRect.anchorMin;
                    origDescAnchorMax = descRect.anchorMax;
                    cachedDescAnchors = true;
                }
            }

            // Resize pageDescription: on plant pages, shrink to a small header strip
            // at the top so the move entry rows can fill the space below.
            if (pageDescription != null && cachedDescAnchors)
            {
                RectTransform descRect = pageDescription.GetComponent<RectTransform>();
                if (descRect != null)
                {
                    if (isPlantPage)
                    {
                        // Thin strip at the top for the italic flavor text.
                        descRect.anchorMin = new Vector2(origDescAnchorMin.x, 0.78f);
                        descRect.anchorMax = origDescAnchorMax;
                    }
                    else
                    {
                        // Restore full area for welcome / tips pages.
                        descRect.anchorMin = origDescAnchorMin;
                        descRect.anchorMax = origDescAnchorMax;
                    }
                }
            }

            // Show or hide the move entries container.
            if (moveEntriesContainer != null)
                moveEntriesContainer.gameObject.SetActive(isPlantPage);

            if (!isPlantPage)
            {
                // Hide all previews and entry texts on non-plant pages.
                if (moveShapePreviews != null)
                    foreach (var img in moveShapePreviews)
                        if (img != null) img.gameObject.SetActive(false);
                if (moveEntryTexts != null)
                    foreach (var txt in moveEntryTexts)
                        if (txt != null) txt.transform.parent.gameObject.SetActive(false);
                return;
            }

            int slotCount = moveEntryTexts != null ? moveEntryTexts.Length : 0;
            int previewCount = moveShapePreviews != null ? moveShapePreviews.Length : 0;
            int maxSlots = Mathf.Max(slotCount, previewCount);
            activePreviewTextures = new Texture2D[maxSlots];

            for (int i = 0; i < maxSlots; i++)
            {
                bool hasMove = i < page.movesOnPage.Length;

                // Show or hide the entire row (the parent of the text element).
                if (i < slotCount && moveEntryTexts[i] != null)
                {
                    Transform row = moveEntryTexts[i].transform.parent;
                    if (row != null)
                        row.gameObject.SetActive(hasMove);
                }

                if (!hasMove)
                {
                    if (i < previewCount && moveShapePreviews[i] != null)
                        moveShapePreviews[i].gameObject.SetActive(false);
                    continue;
                }

                MoveData move = page.movesOnPage[i];

                // --- Build the text for this move ---
                if (i < slotCount && moveEntryTexts[i] != null)
                {
                    string elementColor = GetElementColorHex(move.element);
                    string moveType = move.isDefensiveMove ? "Defense" :
                                      move.isHealingMove ? "Healing" :
                                      "Attack";
                    string powerText = move.basePower > 0 ? $"PWR: {move.basePower}" : "Reduces damage";
                    string shapeName = FormatShapeName(move.drawingShape);

                    string text = $"<b><color={elementColor}>{move.moveName}</color></b>  {moveType}\n";
                    text += $"{move.description}\n";
                    text += $"<size=11><color=#999999>{powerText}</color></size>\n";
                    text += $"<size=11>Draw: <b>{shapeName}</b>  <i>({move.drawingHint})</i></size>";

                    moveEntryTexts[i].text = text;
                }

                // --- Generate the shape preview image ---
                if (i < previewCount && moveShapePreviews[i] != null)
                {
                    Color previewColor = move.primaryColor;
                    previewColor.a = 1f;

                    Texture2D tex = MoveShapePreview.GeneratePreview(move.drawingShape, 120, 120, previewColor);
                    activePreviewTextures[i] = tex;
                    moveShapePreviews[i].texture = tex;
                    moveShapePreviews[i].gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Return a hex color string appropriate for the move's element type.
        /// </summary>
        private static string GetElementColorHex(MoveData.ElementType element)
        {
            switch (element)
            {
                case MoveData.ElementType.Fire:    return "#FF6633";
                case MoveData.ElementType.Grass:   return "#66DD66";
                case MoveData.ElementType.Water:   return "#5599FF";
                default:                           return "#FFFFFF";
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
