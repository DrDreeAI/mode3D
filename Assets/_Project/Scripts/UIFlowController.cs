using System;
using UnityEngine;

namespace Mode3D.UI
{
    public class UIFlowController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject destinationPanel;
        [SerializeField] private GameObject datePanel;
        [SerializeField] private GameObject summaryPanel;
        [SerializeField] private UnityEngine.UI.Text summaryText;

        [Header("References")]
        [SerializeField] private DateRangePicker dateRangePicker;

        public Destination? SelectedDestination { get; private set; }
        public DateTime? SelectedStartDate { get; private set; }
        public DateTime? SelectedEndDate { get; private set; }

        private void Awake()
        {
            if (destinationPanel == null)
            {
                var dp = GameObject.Find("DestinationPanel");
                if (dp != null) destinationPanel = dp;
            }
            if (datePanel == null)
            {
                var dpp = GameObject.Find("DatePanel");
                if (dpp != null) datePanel = dpp;
            }

		if (dateRangePicker == null)
		{
			dateRangePicker = FindFirstObjectByType<DateRangePicker>(FindObjectsInactive.Include);
		}

            ShowDestinationScreen();

            if (dateRangePicker != null)
            {
                dateRangePicker.OnRangeValidated -= HandleRangeValidated;
                dateRangePicker.OnRangeValidated += HandleRangeValidated;
            }
        }

        public void SetDestination(Destination destination)
        {
            SelectedDestination = destination;
        }

        public void GoToDateScreen()
        {
            if (destinationPanel != null) destinationPanel.SetActive(false);
            if (datePanel != null) datePanel.SetActive(true);
            if (summaryPanel != null) summaryPanel.SetActive(false);
        }

        private void ShowDestinationScreen()
        {
            if (destinationPanel != null) destinationPanel.SetActive(true);
            if (datePanel != null) datePanel.SetActive(false);
            if (summaryPanel != null) summaryPanel.SetActive(false);
        }

        private void HandleRangeValidated(DateTime start, DateTime end)
        {
            SelectedStartDate = start.Date;
            SelectedEndDate = end.Date;
            ShowSummary();
        }

        private void ShowSummary()
        {
            if (destinationPanel != null) destinationPanel.SetActive(false);
            if (datePanel != null) datePanel.SetActive(false);
            if (summaryPanel != null) summaryPanel.SetActive(true);

            if (summaryText != null)
            {
                var city = SelectedDestination.HasValue ? SelectedDestination.Value.ToString() : "(ville)";
                var startStr = SelectedStartDate.HasValue ? SelectedStartDate.Value.ToString("dd MMMM") : "?";
                var endStr = SelectedEndDate.HasValue ? SelectedEndDate.Value.ToString("dd MMMM") : "?";
                summaryText.text = $"Vous partez à {city} du {startStr} au {endStr}.";
            }
        }
    }
}


