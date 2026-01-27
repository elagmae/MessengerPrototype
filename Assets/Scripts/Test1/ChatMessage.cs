using TMPro;
using UnityEngine;

public class ChatMessage : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _textField;

    public void SetMessage(string playerName, string message)
    {
        _textField.text = $"<color=grey>{playerName}</color> : {message}";
    }
}
