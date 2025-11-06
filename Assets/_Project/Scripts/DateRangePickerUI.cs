using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// DateRangePickerUI
// Attach this to a GameObject on your Canvas. It will control a calendar panel that lets users pick
// a start and end date. It writes the chosen dates to two TMP_InputField references.
//
// Usage (quick):
// 1. Create a Panel under your Canvas. Add a GridLayoutGroup for the day buttons area (or provide a RectTransform)
// 2. Create two TMP_InputField fields for Start/End and assign them to this component.
// 3. Wire `calendarPanel`, `daysContainer`, `monthLabel`, `prevButton` and `nextButton` in the inspector.
// 4. Optionally create a `dayButtonPrefab` (Unity UI Button with Image and a TextMeshProUGUI child). If left null,
//    the script will create simple buttons at runtime.
// 5. Call `Open()` to show the calendar. The panel will be hidden after confirmation.

public class DateRangePickerUI : MonoBehaviour
{
    [Header("Input fields")]
    public TMP_InputField startInput;
    public TMP_InputField endInput;

    [Header("Calendar UI refs")]
    public GameObject calendarPanel; // parent panel to show/hide
    public RectTransform daysContainer; // where day buttons will be placed (should have GridLayoutGroup)
    public TextMeshProUGUI monthLabel;
    public Button prevButton;
    public Button nextButton;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Optional button prefab (Button with Image + TMP child)")]
    public Button dayButtonPrefab;

    [Header("Appearance")]
    public Color normalColor = Color.white;
    public Color disabledColor = new Color(0.8f, 0.8f, 0.8f);
    public Color selectedColor = new Color(0.2f, 0.6f, 1f);
    public Color inRangeColor = new Color(0.7f, 0.9f, 1f);

    // internal state
    private DateTime visibleMonth;
    private DateTime? selectedStart;
    private DateTime? selectedEnd;
    private List<Button> dayButtons = new List<Button>();

    void Awake()
    {
        if (calendarPanel != null)
            calendarPanel.SetActive(false);

        if (prevButton != null) prevButton.onClick.AddListener(() => ChangeMonth(-1));
        if (nextButton != null) nextButton.onClick.AddListener(() => ChangeMonth(1));
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(() => { calendarPanel.SetActive(false); });

        visibleMonth = DateTime.Today; // starts at current month
    }

    // Call to open the calendar. Optionally pass preselected range.
    public void Open(DateTime? start = null, DateTime? end = null)
    {
        selectedStart = start;
        selectedEnd = end;
        visibleMonth = start ?? DateTime.Today;
        if (calendarPanel != null) calendarPanel.SetActive(true);
        RebuildCalendar();
    }

    private void ChangeMonth(int delta)
    {
        visibleMonth = visibleMonth.AddMonths(delta);
        RebuildCalendar();
    }

    private void RebuildCalendar()
    {
        if (daysContainer == null) return;

        // clear existing
        foreach (var b in dayButtons) Destroy(b.gameObject);
        dayButtons.Clear();

        var firstOfMonth = new DateTime(visibleMonth.Year, visibleMonth.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(visibleMonth.Year, visibleMonth.Month);

        // Display month label (e.g., April 2025)
        if (monthLabel != null) monthLabel.text = firstOfMonth.ToString("MMMM yyyy");

        // Calculate day-of-week offset (assuming week starts Sunday: DayOfWeek.Sunday == 0)
        int startOffset = (int)firstOfMonth.DayOfWeek;

        // create empty placeholders for offset
        for (int i = 0; i < startOffset; i++)
        {
            var empty = CreateDayButton(null);
            SetButtonInteractable(empty, false);
        }

        // create day buttons
        for (int day = 1; day <= daysInMonth; day++)
        {
            var dt = new DateTime(visibleMonth.Year, visibleMonth.Month, day);
            var btn = CreateDayButton(dt);
            dayButtons.Add(btn);
            UpdateButtonVisual(btn, dt);
        }
    }

    private Button CreateDayButton(DateTime? date)
    {
        Button instance = null;
        if (dayButtonPrefab != null)
        {
            instance = Instantiate(dayButtonPrefab, daysContainer);
        }
        else
        {
            // create a simple button with Image + TMP child
            var go = new GameObject(date.HasValue ? $"Day_{date.Value:yyyy_MM_dd}" : "Day_empty", typeof(RectTransform));
            go.transform.SetParent(daysContainer, false);

            var img = go.AddComponent<Image>();
            img.color = normalColor;

            instance = go.AddComponent<Button>();

            // add TMP label
            var txtGO = new GameObject("Label", typeof(RectTransform));
            txtGO.transform.SetParent(go.transform, false);
            var txt = txtGO.AddComponent<TextMeshProUGUI>();
            txt.alignment = TextAlignmentOptions.Center;
            txt.raycastTarget = false;
            txt.fontSize = 18;
            txt.color = Color.black;
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = Vector2.zero;
            txt.rectTransform.offsetMax = Vector2.zero;
            if (date.HasValue) txt.text = date.Value.Day.ToString();
        }

        // wire click
        instance.onClick.RemoveAllListeners();
        if (date.HasValue)
        {
            var captured = date.Value;
            instance.onClick.AddListener(() => OnDayClicked(captured, instance));
        }

        return instance;
    }

    private void SetButtonInteractable(Button btn, bool interactable)
    {
        if (btn == null) return;
        btn.interactable = interactable;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = interactable ? normalColor : disabledColor;
    }

    private void UpdateButtonVisual(Button btn, DateTime dt)
    {
        var img = btn.GetComponent<Image>();
        if (selectedStart.HasValue && selectedStart.Value.Date == dt.Date || selectedEnd.HasValue && selectedEnd.Value.Date == dt.Date)
        {
            if (img != null) img.color = selectedColor;
        }
        else if (selectedStart.HasValue && selectedEnd.HasValue && dt.Date > selectedStart.Value.Date && dt.Date < selectedEnd.Value.Date)
        {
            if (img != null) img.color = inRangeColor;
        }
        else
        {
            if (img != null) img.color = normalColor;
        }
    }

    private void OnDayClicked(DateTime dt, Button btn)
    {
        // if no start selected, set start
        if (!selectedStart.HasValue || (selectedStart.HasValue && selectedEnd.HasValue))
        {
            selectedStart = dt.Date;
            selectedEnd = null;
        }
        else if (selectedStart.HasValue && !selectedEnd.HasValue)
        {
            // if clicked earlier than start, treat as new start
            if (dt.Date < selectedStart.Value.Date)
            {
                selectedEnd = selectedStart;
                selectedStart = dt.Date;
            }
            else
            {
                selectedEnd = dt.Date;
            }
        }

        // refresh visuals
        foreach (var dbtn in dayButtons)
        {
            // try to parse date from name if created dynamically
            DateTime parsed;
            var name = dbtn.gameObject.name;
            if (name.StartsWith("Day_") && DateTime.TryParseExact(name.Substring(4), "yyyy_MM_dd", null, System.Globalization.DateTimeStyles.None, out parsed))
            {
                UpdateButtonVisual(dbtn, parsed);
            }
            else
            {
                // fallback: try label text
                var tmp = dbtn.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null && int.TryParse(tmp.text, out int day))
                {
                    var candidate = new DateTime(visibleMonth.Year, visibleMonth.Month, day);
                    UpdateButtonVisual(dbtn, candidate);
                }
            }
        }
    }

    private void OnConfirm()
    {
        if (selectedStart.HasValue)
        {
            startInput.text = selectedStart.Value.ToString("dd/MM/yyyy");
        }
        if (selectedEnd.HasValue)
        {
            endInput.text = selectedEnd.Value.ToString("dd/MM/yyyy");
        }

        if (calendarPanel != null) calendarPanel.SetActive(false);
    }

    // Optional helper: set a preselected range from two input fields (parse their text)
    public void ParseInputsAndOpen()
    {
        DateTime s, e;
        DateTime? ss = null, ee = null;
        if (startInput != null && DateTime.TryParse(startInput.text, out s)) ss = s;
        if (endInput != null && DateTime.TryParse(endInput.text, out e)) ee = e;
        Open(ss, ee);
    }
}
