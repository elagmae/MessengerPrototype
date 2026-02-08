using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MessageSendingBehaviour : NetworkBehaviour
{
    [SerializeField] private MessageReceiveBehaviour _receiverPrefab;
    private TMP_InputField _messageField;
    public ScrollRect ScrollRect { get;private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.SceneManager.OnSceneEvent += OnSceneLoaded;
    }

    private void OnSceneLoaded(SceneEvent sceneEvent) // If the game scene is load, find the components needed.
    {
        if (sceneEvent.SceneEventType != SceneEventType.SynchronizeComplete && sceneEvent.Scene.name != "Game") return;
        if (ScrollRect != null && _messageField != null) return;

        ScrollRect = FindFirstObjectByType<ScrollRect>();
        _messageField = FindFirstObjectByType<TMP_InputField>();
    }

    public void ResetInput() // Everytime the player sends the message, we empty their input and allow them to type again.
    {
        _messageField.text = "";
        _messageField.Select();
        _messageField.ActivateInputField();
    }

    private void Update()
    {
        // Everytime the player uses the enter key, he sends a message containing his text input.
        if (Input.GetKeyDown(KeyCode.Return)) SendMessageToServerRpc(_messageField.text);
    }

    [ServerRpc] // We first send the message to the server.
    private void SendMessageToServerRpc(string message, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        ReceiveMessageClientRpc(message, senderClientId); // We then provide the messages to every client.
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ReceiveMessageClientRpc(string message, ulong senderClientId) // Spawn message prefab and update its values.
    {
        Canvas.ForceUpdateCanvases();
        
        MessageReceiveBehaviour lastMessage = Instantiate(_receiverPrefab, ScrollRect.content); 

        ScrollRect.content.SetSizeWithCurrentAnchors
        (
            RectTransform.Axis.Vertical, 
            (lastMessage.Image.rectTransform.rect.size.y + 50) * (ScrollRect.content.childCount + 0.2f)
        );

        ScrollRect.verticalNormalizedPosition = 0;

        lastMessage.MessageDisplay.text = message;
        lastMessage.PlayerDisplay.text = $"Player {senderClientId}";

        if (IsOwner) ResetInput();
    }

}
