using TMPro;
using Unity.Netcode;
using UnityEngine;

public class JoinCodeDisplayHandler : MonoBehaviour
{
    private TextMeshProUGUI _codeDisplay;
    private RelayConnectionHandler _relay;

    private void Awake()
    {
        // Shows the join code.
        TryGetComponent(out _codeDisplay);
        NetworkManager.Singleton.gameObject.TryGetComponent(out _relay);
        _codeDisplay.text = _relay.JoinCode;
    }
}
