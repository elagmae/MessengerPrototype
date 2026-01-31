using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatNetwork : NetworkBehaviour
{
    [SerializeField] private Image _messagePanel;

    private TextMeshProUGUI _playerName;
    private TextMeshProUGUI _playerMessage;

    private bool _networkStopped;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        CacheUI();

        GameManager.Instance.Input.Select();
        GameManager.Instance.Input.ActivateInputField();

        _networkStopped = false;
    }

    public override void OnNetworkDespawn()
    {
        _playerName = null;
        _playerMessage = null;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (_networkStopped) return;
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening) return;

        if (Input.GetKeyUp(KeyCode.Return))
        {
            string msg = GameManager.Instance.Input.text;

            if (string.IsNullOrWhiteSpace(msg)) return;

            SendMessageServerRpc(msg);
        }
    }

    [ServerRpc]
    private void SendMessageServerRpc(string message, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        ReceiveMessageClientRpc(message, senderClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ReceiveMessageClientRpc(string message, ulong senderClientId)
    {
        if (IsOwner) return;
        CreateMessageRpc(message, senderClientId);
    }

    private void CacheUI()
    {
        _playerName = _messagePanel.transform
            .GetChild(0)
            .GetComponent<TextMeshProUGUI>();

        _playerMessage = _messagePanel.transform
            .GetChild(1)
            .GetComponent<TextMeshProUGUI>();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CreateMessageRpc(string message, ulong senderClientId)
    {
        Image img = Instantiate(_messagePanel, GameManager.Instance.Container, false);

        if (GameManager.Instance.Container.childCount >= 2)
        {
            Image lastImg = GameManager.Instance.Container.GetChild(GameManager.Instance.Container.childCount - 2).GetComponent<Image>();
            Color color = lastImg.color;
            color.a = 0.3f;
            lastImg.color = color;
            lastImg.transform.localScale = Vector3.one * 0.9f;
        }

        TextMeshProUGUI name = img.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI text = img.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        name.text = $"Player {senderClientId}";
        text.text = message;

        ResizeContainerRpc(img.rectTransform.rect.size.y/2 + 30f);

        GameManager.Instance.ScrollRect.verticalNormalizedPosition = 0;
        GameManager.Instance.Bar.value = 0;

        Canvas.ForceUpdateCanvases();

        ResetInput();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ResizeContainerRpc(float addedHeight)
    {
        RectTransform container = GameManager.Instance.Container;

        container.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            container.rect.height + addedHeight
        );
    }

    private void ResetInput()
    {
        GameManager.Instance.Input.text = "";
        GameManager.Instance.Input.Select();
        GameManager.Instance.Input.ActivateInputField();
    }

    [Rpc(SendTo.Everyone)]
    public void StopNetworkRpc()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            RpcParams rpcParams = default;
            NetworkManager.Singleton.DisconnectClient(rpcParams.Receive.SenderClientId);

            _networkStopped = true;
            NetworkManager.Singleton.Shutdown();
        }
    }

    public override void OnDestroy()
    {
        StopNetworkRpc();
        base.OnDestroy();
    }
}
