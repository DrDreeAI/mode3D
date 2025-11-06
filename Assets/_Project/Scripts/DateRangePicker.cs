using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.UI
{
    public class DateRangePicker : MonoBehaviour
    {
        [Header("Calendar Container (empty parent)")]
        [SerializeField] private RectTransform calendarContainer;

        [Header("Controls")] 
        [SerializeField] private Button validateButton;

        [Header("Styles")] 
        [SerializeField] private Color defaultColor = new Color(0.92f, 0.92f, 0.92f);
        [SerializeField] private Color selectedColor = new Color(0.25f, 0.56f, 0.95f);
        [SerializeField] private Color inRangeColor = new Color(0.70f, 0.83f, 1.00f);
        [SerializeField] private Color textDefaultColor = Color.black;
        [SerializeField] private Color textSelectedColor = Color.white;

        public event Action<DateTime, DateTime> OnRangeValidated;

        private readonly List<DayCell> dayCells = new List<DayCell>();
        private DateTime? startSelection;
        private DateTime? endSelection;

        private const int DaysInWeek = 7;

        private void Awake()
        {
            if (calendarContainer == null)
            {
                var cc = transform.Find("CalendarContainer");
                if (cc != null) calendarContainer = (RectTransform)cc;
            }
            if (validateButton == null)
            {
                var vb = transform.Find("ValidateDateButton");
                if (vb != null) validateButton = vb.GetComponent<Button>();
            }

            if (validateButton != null)
            {
                validateButton.onClick.RemoveAllListeners();
                validateButton.onClick.AddListener(ValidateRange);
            }

            BuildCalendar(DateTime.Today);
        }

        private void BuildCalendar(DateTime referenceDate)
        {
            if (calendarContainer == null)
            {
                Debug.LogError("DateRangePicker: calendarContainer is not assigned.");
                return;
            }

            // Clear existing
            foreach (Transform child in calendarContainer)
            {
                Destroy(child.gameObject);
            }
            dayCells.Clear();

            // Build a simple single-month grid (current month). For a production UI, render multiple months.
            var firstOfMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var firstDayOfGrid = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);
            var totalCells = 6 * DaysInWeek; // 6 weeks grid

            for (int i = 0; i < totalCells; i++)
            {
                var date = firstDayOfGrid.AddDays(i);
                var cell = CreateDayButton(date, date.Month == referenceDate.Month);
                dayCells.Add(cell);
            }
        }

        private DayCell CreateDayButton(DateTime date, bool isCurrentMonth)
        {
            var go = new GameObject($"Day_{date:yyyyMMdd}", typeof(RectTransform), typeof(Button), typeof(Image));
            go.transform.SetParent(calendarContainer, false);

            var image = go.GetComponent<Image>();
            image.color = defaultColor * (isCurrentMonth ? 1f : 0.85f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => OnDayClicked(date));

            // Text label
            var textGo = new GameObject("Label", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<Text>();
            text.text = date.Day.ToString();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = textDefaultColor;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var textRect = (RectTransform)textGo.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Layout (uniform grid via GridLayoutGroup on container is recommended). If not present, set size.
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(64, 48);

            return new DayCell
            {
                date = date,
                button = button,
                image = image,
                label = text
            };
        }

        private void OnDayClicked(DateTime date)
        {
            if (!startSelection.HasValue || (startSelection.HasValue && endSelection.HasValue))
            {
                startSelection = date.Date;
                endSelection = null;
            }
            else if (startSelection.HasValue && !endSelection.HasValue)
            {
                if (date.Date < startSelection.Value)
                {
                    endSelection = startSelection.Value;
                    startSelection = date.Date;
                }
                else
                {
                    endSelection = date.Date;
                }
            }

            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            foreach (var cell in dayCells)
            {
                var isSelectedStart = startSelection.HasValue && cell.date.Date == startSelection.Value;
                var isSelectedEnd = endSelection.HasValue && cell.date.Date == endSelection.Value;
                var inRange = startSelection.HasValue && endSelection.HasValue && cell.date.Date > startSelection.Value && cell.date.Date < endSelection.Value;

                if (isSelectedStart || isSelectedEnd)
                {
                    cell.image.color = selectedColor;
                    cell.label.color = textSelectedColor;
                }
                else if (inRange)
                {
                    cell.image.color = inRangeColor;
                    cell.label.color = textDefaultColor;
                }
                else
                {
                    cell.image.color = defaultColor;
                    cell.label.color = textDefaultColor;
                }
            }
        }

        private void ValidateRange()
        {
            if (!startSelection.HasValue || !endSelection.HasValue)
            {
                Debug.LogWarning("DateRangePicker: Please select a start and end date (two clicks).");
                return;
            }

            OnRangeValidated?.Invoke(startSelection.Value, endSelection.Value);
        }

        private struct DayCell
        {
            public DateTime date;
            public Button button;
            public Image image;
            public Text label;
        }
    }
}


