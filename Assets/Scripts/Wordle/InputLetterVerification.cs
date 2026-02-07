using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public class InputLetterVerification : MonoBehaviour
{
    public TMP_InputField input;

    void Start() => input.onValueChanged.AddListener(FilterInput);

    void FilterInput(string value)
    {
        // Garde uniquement les lettres A-Z (sans accents)
        string filtered = Regex.Replace(value, "[^a-zA-Z]", "");

        // Force majuscule (optionnel)
        filtered = filtered.ToUpper();

        if (filtered != value)
        {
            input.SetTextWithoutNotify(filtered);
            input.caretPosition = filtered.Length;
        }
    }
}
