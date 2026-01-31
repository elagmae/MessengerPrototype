using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MessageSendingBehaviour : NetworkBehaviour
{
    [SerializeField] private MessageReceiveBehaviour _receiverPrefab;
    private MessageShowerBehaviour _messageShower;
    private TMP_InputField _messageField;
    public ScrollRect ScrollRect { get;private set; }

    private void Awake() => TryGetComponent(out _messageShower);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.SceneManager.OnSceneEvent += OnSceneLoaded;
    }

    private void OnSceneLoaded(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType != SceneEventType.SynchronizeComplete && sceneEvent.Scene.name != "Game") return;

        ScrollRect = FindFirstObjectByType<ScrollRect>();
        _messageField = FindFirstObjectByType<TMP_InputField>();
    }

    public void ResetInput()
    {
        _messageField.text = "";
        _messageField.Select();
        _messageField.ActivateInputField();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) SendMessageToServerRpc(_messageField.text);
    }

    private void SendMessage(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || SceneManager.GetActiveScene().name != "Game") return;
        print("sending...");
        SendMessageToServerRpc(_messageField.text);
    }

    [ServerRpc]
    private void SendMessageToServerRpc(string message, ServerRpcParams rpcParams = default)
    {
        print("to server...");
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        ReceiveMessageClientRpc(message, senderClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ReceiveMessageClientRpc(string message, ulong senderClientId)
    {
        print("to client..." + senderClientId);

        MessageReceiveBehaviour lastMessage = Instantiate(_receiverPrefab, ScrollRect.content);
        lastMessage.MessageDisplay.text = message;
        lastMessage.PlayerDisplay.text = $"Player {senderClientId}";

        ResetInput();
    }

}
