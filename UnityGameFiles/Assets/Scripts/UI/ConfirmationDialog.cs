using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace SketchBlossom.UI
{
    /// <summary>
    /// Reusable confirmation dialog for important actions
    /// </summary>
    public class ConfirmationDialog : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject dialogOverlay;
        [SerializeField] private GameObject dialogWindow;

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI cancelButtonText;

        private Action onConfirm;
        private Action onCancel;

        private void Start()
        {
            SetupButtons();
            Hide();
        }

        private void SetupButtons()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
        }

        /// <summary>
        /// Shows the confirmation dialog with custom message and callbacks
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="confirmText">Text for confirm button</param>
        /// <param name="cancelText">Text for cancel button</param>
        /// <param name="onConfirmCallback">Callback when confirmed</param>
        /// <param name="onCancelCallback">Optional callback when cancelled</param>
        public void Show(string title, string message, string confirmText = "Confirm",
                        string cancelText = "Cancel", Action onConfirmCallback = null,
                        Action onCancelCallback = null)
        {
            // Set text content
            if (titleText != null)
                titleText.text = title;

            if (messageText != null)
                messageText.text = message;

            if (confirmButtonText != null)
                confirmButtonText.text = confirmText;

            if (cancelButtonText != null)
                cancelButtonText.text = cancelText;

            // Set callbacks
            onConfirm = onConfirmCallback;
            onCancel = onCancelCallback;

            // Show dialog
            if (dialogOverlay != null)
                dialogOverlay.SetActive(true);

            if (dialogWindow != null)
                dialogWindow.SetActive(true);

            Debug.Log($"[ConfirmationDialog] Showing dialog: {title}");
        }

        /// <summary>
        /// Hides the confirmation dialog
        /// </summary>
        public void Hide()
        {
            if (dialogOverlay != null)
                dialogOverlay.SetActive(false);

            if (dialogWindow != null)
                dialogWindow.SetActive(false);

            // Clear callbacks
            onConfirm = null;
            onCancel = null;

            Debug.Log("[ConfirmationDialog] Dialog hidden");
        }

        private void OnConfirmClicked()
        {
            Debug.Log("[ConfirmationDialog] Confirmed");
            onConfirm?.Invoke();
            Hide();
        }

        private void OnCancelClicked()
        {
            Debug.Log("[ConfirmationDialog] Cancelled");
            onCancel?.Invoke();
            Hide();
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (confirmButton != null)
                confirmButton.onClick.RemoveAllListeners();

            if (cancelButton != null)
                cancelButton.onClick.RemoveAllListeners();
        }
    }
}
