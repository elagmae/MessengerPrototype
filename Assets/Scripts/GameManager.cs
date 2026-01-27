using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [field:SerializeField]
    public RectTransform Container { get; set; }
    [field: SerializeField]
    public TMP_InputField Input { get; set; }
    [field : SerializeField]
    public Scrollbar Bar { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }
}
