using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageDisplayHandler : MonoBehaviour
{
    public TMP_InputField MessageInputField { get;private set; }
    public ScrollRect ScrollRect { get; private set; }

    public void AssignDisplays()
    {
        MessageInputField = FindFirstObjectByType<TMP_InputField>();
        ScrollRect = FindFirstObjectByType<ScrollRect>();
    }
}
