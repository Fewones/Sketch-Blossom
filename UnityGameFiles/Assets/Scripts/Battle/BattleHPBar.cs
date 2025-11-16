using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Displays and animates HP bar for battle units
    /// </summary>
    public class BattleHPBar : MonoBehaviour
    {
        [Header("HP Bar Components")]
        [SerializeField] private Image hpFillImage;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI unitNameText;

        [Header("HP Colors")]
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color mediumHealthColor = Color.yellow;
        [SerializeField] private Color lowHealthColor = Color.red;

        [Header("Animation Settings")]
        [SerializeField] private float animationSpeed = 2f;
        [SerializeField] private bool animateChanges = true;

        private int currentHP;
        private int maxHP;
        private float targetFillAmount;
        private float currentFillAmount;

        private void Update()
        {
            if (animateChanges && hpFillImage != null)
            {
                // Smoothly animate HP bar changes
                if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
                {
                    currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * animationSpeed);
                    hpFillImage.fillAmount = currentFillAmount;

                    // Update color based on fill amount
                    UpdateHPColor(currentFillAmount);
                }
            }
        }

        /// <summary>
        /// Initialize the HP bar with unit data
        /// </summary>
        public void Initialize(string unitName, int maxHealth)
        {
            maxHP = maxHealth;
            currentHP = maxHealth;

            if (unitNameText != null)
                unitNameText.text = unitName;

            targetFillAmount = 1f;
            currentFillAmount = 1f;

            // Ensure HP bar components are visible and enabled
            EnsureVisible();

            UpdateDisplay();

            Debug.Log($"BattleHPBar.Initialize: {unitName} - HP:{currentHP}/{maxHP}, FillAmount:{currentFillAmount}");
        }

        /// <summary>
        /// Ensure the HP bar is visible and all components are enabled
        /// </summary>
        public void EnsureVisible()
        {
            // Enable this GameObject
            if (gameObject != null && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Debug.LogWarning("BattleHPBar: Had to activate HP bar GameObject!");
            }

            // Ensure fill image is enabled
            if (hpFillImage != null)
            {
                if (!hpFillImage.enabled)
                {
                    hpFillImage.enabled = true;
                    Debug.LogWarning("BattleHPBar: Had to enable hpFillImage!");
                }

                if (hpFillImage.gameObject != null && !hpFillImage.gameObject.activeSelf)
                {
                    hpFillImage.gameObject.SetActive(true);
                    Debug.LogWarning("BattleHPBar: Had to activate hpFillImage GameObject!");
                }
            }
            else
            {
                Debug.LogError("BattleHPBar: hpFillImage is NULL!");
            }

            // Ensure HP text is enabled
            if (hpText != null && !hpText.enabled)
            {
                hpText.enabled = true;
            }

            // Ensure unit name text is enabled
            if (unitNameText != null && !unitNameText.enabled)
            {
                unitNameText.enabled = true;
            }
        }

        /// <summary>
        /// Update HP value
        /// </summary>
        public void SetHP(int newHP)
        {
            int oldHP = currentHP;
            currentHP = Mathf.Clamp(newHP, 0, maxHP);
            targetFillAmount = maxHP > 0 ? (float)currentHP / maxHP : 0f;

            Debug.Log($"BattleHPBar.SetHP: {oldHP} -> {currentHP} (target fill: {targetFillAmount:F2})");

            if (!animateChanges && hpFillImage != null)
            {
                currentFillAmount = targetFillAmount;
                hpFillImage.fillAmount = currentFillAmount;
                UpdateHPColor(currentFillAmount);
                Debug.Log($"BattleHPBar: Immediately set fill amount to {currentFillAmount:F2}");
            }
            else if (hpFillImage != null)
            {
                Debug.Log($"BattleHPBar: Animating from {currentFillAmount:F2} to {targetFillAmount:F2}");
            }

            UpdateDisplay();
        }

        /// <summary>
        /// Update HP by a delta amount (positive for heal, negative for damage)
        /// </summary>
        public void ModifyHP(int delta)
        {
            Debug.Log($"BattleHPBar.ModifyHP: Applying {delta:+0;-#} HP change");
            SetHP(currentHP + delta);
        }

        /// <summary>
        /// Get current HP
        /// </summary>
        public int GetCurrentHP()
        {
            return currentHP;
        }

        /// <summary>
        /// Get max HP
        /// </summary>
        public int GetMaxHP()
        {
            return maxHP;
        }

        /// <summary>
        /// Check if unit is alive
        /// </summary>
        public bool IsAlive()
        {
            return currentHP > 0;
        }

        /// <summary>
        /// Set unit name
        /// </summary>
        public void SetUnitName(string name)
        {
            if (unitNameText != null)
                unitNameText.text = name;
        }

        private void UpdateDisplay()
        {
            if (hpText != null)
            {
                hpText.text = $"{currentHP} / {maxHP}";
                Debug.Log($"BattleHPBar.UpdateDisplay: Text updated to '{hpText.text}'");
            }
            else
            {
                Debug.LogWarning("BattleHPBar.UpdateDisplay: hpText is NULL!");
            }

            if (hpFillImage != null && !animateChanges)
            {
                hpFillImage.fillAmount = targetFillAmount;
                UpdateHPColor(targetFillAmount);
                Debug.Log($"BattleHPBar.UpdateDisplay: Fill amount set to {targetFillAmount:F2}, Color: {hpFillImage.color}");
            }
            else if (hpFillImage == null)
            {
                Debug.LogError("BattleHPBar.UpdateDisplay: hpFillImage is NULL!");
            }
        }

        private void UpdateHPColor(float fillAmount)
        {
            if (hpFillImage == null) return;

            if (fillAmount > 0.5f)
            {
                // Full to medium health
                hpFillImage.color = Color.Lerp(mediumHealthColor, fullHealthColor, (fillAmount - 0.5f) * 2f);
            }
            else
            {
                // Low to medium health
                hpFillImage.color = Color.Lerp(lowHealthColor, mediumHealthColor, fillAmount * 2f);
            }
        }

        /// <summary>
        /// Pulse animation for taking damage
        /// </summary>
        public void PlayDamageAnimation()
        {
            // Can be extended with animation
            StartCoroutine(DamagePulse());
        }

        private System.Collections.IEnumerator DamagePulse()
        {
            Transform barTransform = hpFillImage != null ? hpFillImage.transform : transform;
            Vector3 originalScale = barTransform.localScale;

            // Pulse effect
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float scale = 1f + Mathf.Sin((elapsed / duration) * Mathf.PI) * 0.1f;
                barTransform.localScale = originalScale * scale;
                yield return null;
            }

            barTransform.localScale = originalScale;
        }

        /// <summary>
        /// Set custom HP bar color
        /// </summary>
        public void SetCustomColor(Color color)
        {
            if (hpFillImage != null)
                hpFillImage.color = color;
        }
    }
}
