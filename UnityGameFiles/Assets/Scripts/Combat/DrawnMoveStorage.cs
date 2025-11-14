using UnityEngine;
using System.Collections.Generic;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Stores captured move drawings for use as attack animations.
    /// Manages a queue of drawings that cycle through successive attacks.
    /// </summary>
    public class DrawnMoveStorage : MonoBehaviour
    {
        public static DrawnMoveStorage Instance { get; private set; }

        [Header("Storage Settings")]
        [SerializeField] private int maxStoredMoves = 10;
        [SerializeField] private bool cycleDrawings = true;

        // Queue of captured move drawings
        private Queue<Texture2D> moveDrawingQueue = new Queue<Texture2D>();
        private int currentDrawingIndex = 0;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// Add a newly captured move drawing to the queue
        /// </summary>
        public void AddMoveDrawing(Texture2D moveTexture)
        {
            if (moveTexture == null)
            {
                Debug.LogWarning("DrawnMoveStorage: Attempted to add null texture!");
                return;
            }

            // If we've reached max capacity, remove the oldest drawing
            if (moveDrawingQueue.Count >= maxStoredMoves)
            {
                Texture2D oldestTexture = moveDrawingQueue.Dequeue();
                if (oldestTexture != null)
                {
                    Destroy(oldestTexture);
                }
                Debug.Log($"DrawnMoveStorage: Removed oldest drawing (queue was full)");
            }

            moveDrawingQueue.Enqueue(moveTexture);
            Debug.Log($"DrawnMoveStorage: Added move drawing. Queue size: {moveDrawingQueue.Count}");
        }

        /// <summary>
        /// Get the next move drawing for an attack animation.
        /// Cycles through drawings if cycleDrawings is enabled.
        /// </summary>
        public Texture2D GetNextMoveDrawing()
        {
            if (moveDrawingQueue.Count == 0)
            {
                Debug.LogWarning("DrawnMoveStorage: No move drawings available!");
                return null;
            }

            // Convert queue to array to access by index
            Texture2D[] drawings = moveDrawingQueue.ToArray();

            // Get current drawing
            Texture2D selectedDrawing = drawings[currentDrawingIndex % drawings.Length];

            // Cycle to next drawing if enabled
            if (cycleDrawings)
            {
                currentDrawingIndex = (currentDrawingIndex + 1) % drawings.Length;
            }

            Debug.Log($"DrawnMoveStorage: Retrieved drawing {currentDrawingIndex}/{drawings.Length}");
            return selectedDrawing;
        }

        /// <summary>
        /// Get the most recent move drawing without cycling
        /// </summary>
        public Texture2D GetLatestMoveDrawing()
        {
            if (moveDrawingQueue.Count == 0)
            {
                Debug.LogWarning("DrawnMoveStorage: No move drawings available!");
                return null;
            }

            // Return the most recent drawing
            Texture2D[] drawings = moveDrawingQueue.ToArray();
            return drawings[drawings.Length - 1];
        }

        /// <summary>
        /// Clear all stored move drawings
        /// </summary>
        public void ClearAllDrawings()
        {
            while (moveDrawingQueue.Count > 0)
            {
                Texture2D texture = moveDrawingQueue.Dequeue();
                if (texture != null)
                {
                    Destroy(texture);
                }
            }
            currentDrawingIndex = 0;
            Debug.Log("DrawnMoveStorage: Cleared all move drawings");
        }

        /// <summary>
        /// Get the number of stored drawings
        /// </summary>
        public int GetDrawingCount()
        {
            return moveDrawingQueue.Count;
        }

        /// <summary>
        /// Check if we have any drawings stored
        /// </summary>
        public bool HasDrawings()
        {
            return moveDrawingQueue.Count > 0;
        }

        private void OnDestroy()
        {
            // Clean up textures
            ClearAllDrawings();
        }
    }
}
