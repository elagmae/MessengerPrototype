using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public class InputLetterVerification : MonoBehaviour
{
    public TMP_InputField input;

    void Start() => input.onValueChanged.AddListener(FilterInput);

    void FilterInput(string value)
    {
        // Only allows letters from A to Z.
        string filtered = Regex.Replace(value, "[^a-zA-Z]", "");

        // Forces upper case.
        filtered = filtered.ToUpper();

        if (filtered != value)
        {
            input.SetTextWithoutNotify(filtered);
            input.caretPosition = filtered.Length;
        }
    }
}
