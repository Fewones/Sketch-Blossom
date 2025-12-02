using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Round timer for the player turn.
    /// Runs only during PlayerDrawing and notifies the DrawingBattleSceneManager on timeout.
    /// </summary>
    public class BattleTimer : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFillImage;
        [SerializeField] private Image warningFlashOverlay;

        [Header("Timing (seconds)")]
        [SerializeField] private float easyModeDuration = 30f; // normal battles
        [SerializeField] private float hardModeDuration = 15f; // high difficulty battles

        [Header("Warning Thresholds")]
        [SerializeField] private float firstWarningTime = 10f;
        [SerializeField] private float secondWarningTime = 5f;

        [Header("Colors")]
        [SerializeField] private Color safeColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color dangerColor = Color.red;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip tickClip;
        [SerializeField] private AudioClip urgentTickClip;
        [SerializeField] private AudioClip timeoutClip;

        [Header("Battle Manager Reference")]
        [SerializeField] private DrawingBattleSceneManager battleManager;

        private float currentDuration;
        private float remainingTime;
        private bool isRunning;
        private bool firstWarningTriggered;
        private bool secondWarningTriggered;
        private bool useHardTimeout;
        private float tickAccumulator;

        /// <summary>
        /// Starts the timer for the player turn.
        /// isHardMode = false -> auto submit on timeout
        /// isHardMode = true  -> player loses the entire turn on timeout
        /// </summary>
        public void StartPlayerTurnTimer(bool isHardMode)
        {
            useHardTimeout = isHardMode;
            currentDuration = isHardMode ? hardModeDuration : easyModeDuration;

            remainingTime = currentDuration;
            firstWarningTriggered = false;
            secondWarningTriggered = false;
            tickAccumulator = 0f;

            isRunning = true;
            ShowUI();
            UpdateUI();
        }

        public void Stop()
        {
            isRunning = false;
            HideUI();
        }

        public void Pause()
        {
            isRunning = false;
        }

        private void Update()
        {
            if (!isRunning) return;

            remainingTime -= Time.deltaTime;
            if (remainingTime < 0f) remainingTime = 0f;

            UpdateUI();
            HandleWarnings();
            HandleTickSound();

            if (remainingTime <= 0f)
            {
                HandleTimeout();
            }
        }

        private void UpdateUI()
        {
            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(remainingTime).ToString("0");
            }

            if (timerFillImage != null)
            {
                float normalized = currentDuration > 0f ? remainingTime / currentDuration : 0f;
                timerFillImage.fillAmount = normalized;

                if (remainingTime <= secondWarningTime)
                    timerFillImage.color = dangerColor;
                else if (remainingTime <= firstWarningTime)
                    timerFillImage.color = warningColor;
                else
                    timerFillImage.color = safeColor;
            }
        }

        private void HandleWarnings()
        {
            if (!firstWarningTriggered && remainingTime <= firstWarningTime)
            {
                firstWarningTriggered = true;
                TriggerWarningFlash();
            }

            if (!secondWarningTriggered && remainingTime <= secondWarningTime)
            {
                secondWarningTriggered = true;
                TriggerWarningFlash();
            }
        }

        private void TriggerWarningFlash()
        {
            if (warningFlashOverlay == null) return;

            warningFlashOverlay.gameObject.SetActive(true);
            CancelInvoke(nameof(HideWarningFlash));
            Invoke(nameof(HideWarningFlash), 0.25f);
        }

        private void HideWarningFlash()
        {
            if (warningFlashOverlay != null)
            {
                warningFlashOverlay.gameObject.SetActive(false);
            }
        }

        private void HandleTickSound()
        {
            if (audioSource == null || tickClip == null) return;
            if (remainingTime <= 0f) return;

            tickAccumulator += Time.deltaTime;
            if (tickAccumulator >= 1f)
            {
                tickAccumulator = 0f;

                AudioClip clipToPlay =
                    remainingTime <= secondWarningTime && urgentTickClip != null
                    ? urgentTickClip
                    : tickClip;

                audioSource.PlayOneShot(clipToPlay);
            }
        }

        private void HandleTimeout()
        {
            isRunning = false;
            HideUI();

            if (audioSource != null && timeoutClip != null)
            {
                audioSource.PlayOneShot(timeoutClip);
            }

            if (battleManager == null)
            {
                Debug.LogWarning("BattleTimer: no DrawingBattleSceneManager referenced!");
                return;
            }

            if (useHardTimeout)
            {
                battleManager.OnPlayerTurnTimedOut_Hard();
            }
            else
            {
                battleManager.OnPlayerTurnTimedOut_AutoSubmit();
            }
        }

        private void HideUI()
        {
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (timerFillImage != null) timerFillImage.gameObject.SetActive(false);
            if (warningFlashOverlay != null) warningFlashOverlay.gameObject.SetActive(false);
        }

        private void ShowUI()
        {
            if (timerText != null) timerText.gameObject.SetActive(true);
            if (timerFillImage != null) timerFillImage.gameObject.SetActive(true);
        }
    }
}
