using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Small panel shown below the available-moves list after each drawing attempt.
    /// Displays a confidence bar for every candidate move — sorted highest first —
    /// and highlights the best match.  Works with any moves the current plant has
    /// available; only those moves appear in the list.
    ///
    /// Setup in the Inspector
    /// ──────────────────────
    /// 1. Create a child Panel under the player-turn UI group, positioned just
    ///    below the available-moves text.
    /// 2. Assign that Panel to panelRoot.
    /// 3. Add a TextMeshProUGUI "Header" child  → assign to headerText.
    /// 4. Add a TextMeshProUGUI "Content" child → assign to contentText.
    ///    (Enable Rich Text on the TMP component; a monospace font looks best.)
    /// 5. The panel starts hidden; it becomes visible after every recognition
    ///    attempt and hides automatically when the player begins their next turn.
    /// </summary>
    public class MoveConfidenceDisplay : MonoBehaviour
    {
        [Header("Panel Root")]
        [Tooltip("The whole confidence panel GameObject — toggled on/off.")]
        [SerializeField] private GameObject panelRoot;

        [Header("Text Elements")]
        [Tooltip("Title line: e.g. 'Move Detected!' or 'Not Recognised'")]
        [SerializeField] private TextMeshProUGUI headerText;

        [Tooltip("Body text that lists every move with a confidence bar.")]
        [SerializeField] private TextMeshProUGUI contentText;

        // ── Appearance ──────────────────────────────────────────────────────

        private const int BarWidth = 10; // number of block characters in each bar

        // Rich-text colours
        private const string ColourMatch   = "#0B620B"; // green  – recognised move
        private const string ColourPartial = "#FFAA44"; // amber  – best guess but below threshold
        private const string ColourDim     = "#AAAAAA"; // grey   – other candidates

        // ── Lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            Hide();
        }

        // ── Public API ──────────────────────────────────────────────────────

        /// <summary>
        /// Show confidence scores for all evaluated moves.
        /// Call this immediately after MovesetDetector.DetectMoveWithCLIP() returns.
        /// </summary>
        /// <param name="scores">Map of move type → combined confidence (0–1).</param>
        /// <param name="detectedMove">The move the detector picked as best match.</param>
        /// <param name="wasRecognized">True if confidence exceeded the acceptance threshold.</param>
        /// <param name="qualityRating">Human-readable quality label (e.g. "Excellent").</param>
        /// <param name="damageMultiplier">Damage multiplier derived from drawing quality.</param>
        /// <param name="moveNames">Optional map of move type → plant-specific display name.</param>
        public void ShowScores(
            Dictionary<MoveData.MoveType, float> scores,
            MoveData.MoveType detectedMove,
            bool wasRecognized,
            string qualityRating,
            float damageMultiplier,
            Dictionary<MoveData.MoveType, string> moveNames = null)
        {
            if (panelRoot == null || contentText == null) return;

            // Header
            if (headerText != null)
                headerText.text = wasRecognized ? "Move Detected!" : "Not Recognised";

            // Body — sort descending so the best guess is always at the top
            var sorted = scores
                .Where(kv => kv.Key != MoveData.MoveType.Unknown)
                .OrderByDescending(kv => kv.Value)
                .ToList();

            var sb = new System.Text.StringBuilder();

            foreach (var kv in sorted)
            {
                bool   isBest  = kv.Key == detectedMove;
                bool   isMatch = isBest && wasRecognized;
                float  pct     = Mathf.Clamp01(kv.Value);
                string bar     = BuildBar(pct);
                string name    = (moveNames != null && moveNames.ContainsKey(kv.Key))
                                     ? moveNames[kv.Key]
                                     : FriendlyName(kv.Key);
                int    pctInt  = Mathf.RoundToInt(pct * 100f);

                string prefix = isMatch ? "✓" : (isBest ? "?" : " ");
                string colour = isMatch ? ColourMatch : (isBest ? ColourPartial : ColourDim);
                string bold   = (isBest) ? "<b>" : "";
                string unbold = (isBest) ? "</b>" : "";

                sb.AppendLine(
                    $"<color={colour}>{bold}{prefix} {name,-13} {bar} {pctInt,3}%{unbold}</color>");
            }

            // Quality footer — only meaningful when the move was accepted
            if (wasRecognized)
            {
                sb.AppendLine();
                sb.Append($"<color=#000000>Quality: <b>{qualityRating}</b>" +
                           $"  ({damageMultiplier:F1}× dmg)</color>");
            }

            contentText.text = sb.ToString();
            panelRoot.SetActive(true);
        }

        /// <summary>
        /// Hide the confidence panel (call at the start of each new player turn
        /// and when the player clears their drawing).
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private static string BuildBar(float value)
        {
            int filled = Mathf.Clamp(Mathf.RoundToInt(value * BarWidth), 0, BarWidth);
            return "[" + new string('█', filled) + new string('░', BarWidth - filled) + "]";
        }

        private static string FriendlyName(MoveData.MoveType moveType)
        {
            switch (moveType)
            {
                case MoveData.MoveType.Block:       return "Block";
                case MoveData.MoveType.Sting:       return "Sting";
                case MoveData.MoveType.Cut:         return "Cut";
                case MoveData.MoveType.Bump:        return "Bump";
                case MoveData.MoveType.Fireball:    return "Fireball";
                case MoveData.MoveType.FlameWave:   return "Flame Wave";
                case MoveData.MoveType.Burn:        return "Burn";
                case MoveData.MoveType.VineWhip:    return "Vine Whip";
                case MoveData.MoveType.LeafStorm:   return "Leaf Storm";
                case MoveData.MoveType.RootAttack:  return "Root Attack";
                case MoveData.MoveType.WaterSplash: return "Water Splash";
                case MoveData.MoveType.Bubble:      return "Bubble";
                case MoveData.MoveType.HealingWave: return "Healing Wave";
                default:                            return moveType.ToString();
            }
        }
    }
}
