using UnityEngine;
using TMPro;
using System;

public class DatePickerUI : MonoBehaviour
{
    public TMP_InputField outputField;

    public void OpenPicker()
    {
        // Utilise la date actuelle (AndroidDatePicker n'est pas disponible)
        outputField.text = DateTime.Now.ToString("dd/MM/yyyy");
        Debug.Log("Date sélectionnée : " + outputField.text);
    }

    private void OnDateSelected(DateTime value)
    {
        outputField.text = value.ToString("dd/MM/yyyy");
        Debug.Log("Date choisie : " + outputField.text);
    }
}
