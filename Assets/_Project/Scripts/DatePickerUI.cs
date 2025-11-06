using UnityEngine;
using TMPro;
using System;

public class DatePickerUI : MonoBehaviour
{
    public TMP_InputField outputField;

    public void OpenPicker()
    {
#if UNITY_EDITOR
        // Simule la date dans Unity Editor
        outputField.text = DateTime.Now.ToString("dd/MM/yyyy");
        Debug.Log("Date simulée : " + outputField.text);
#elif UNITY_ANDROID
        // Appelle le vrai date picker Android
        var picker = new AndroidDatePicker();
        picker.Show(DateTime.Now, OnDateSelected);
#else
        // Fallback pour autres plateformes
        outputField.text = DateTime.Now.ToString("dd/MM/yyyy");
#endif
    }

    private void OnDateSelected(DateTime value)
    {
        outputField.text = value.ToString("dd/MM/yyyy");
        Debug.Log("Date choisie : " + outputField.text);
    }
}
