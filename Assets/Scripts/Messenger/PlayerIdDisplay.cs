using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerIdDisplay : MonoBehaviour
{
    private TextMeshProUGUI _nameDisplay;

    private void Awake()
    {
        TryGetComponent(out _nameDisplay);
        _nameDisplay.text = $"Player {NetworkManager.Singleton.LocalClientId}";
    }
}
