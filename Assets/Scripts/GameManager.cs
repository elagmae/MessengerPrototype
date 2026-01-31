using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [field:SerializeField] public TMP_InputField CodeField { get; private set; }
    [field: SerializeField] public TextMeshProUGUI CodeDisplay { get; private set; }
    [field:SerializeField] public RectTransform Container { get; set; }
    [field: SerializeField] public TMP_InputField Input { get; set; }
    [field : SerializeField] public Scrollbar Bar { get; set; }
    [field: SerializeField] public ScrollRect ScrollRect { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }
}
