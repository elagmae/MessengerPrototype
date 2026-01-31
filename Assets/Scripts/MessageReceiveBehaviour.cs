using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MessageReceiveBehaviour : MonoBehaviour
{
    public Image Image { get; private set; }
    [field: SerializeField] public TextMeshProUGUI MessageDisplay { get; private set; }
    [field:SerializeField] public TextMeshProUGUI PlayerDisplay { get; private set; }

    private void Awake()
    {
        Image = GetComponent<Image>();
    }
}
