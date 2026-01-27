using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class ChatBehaviour : NetworkBehaviour
{
    [SerializeField]
    private TMP_InputField _clientInput;
    [SerializeField]
    private RectTransform _messageParent;
    [SerializeField]
    private ChatMessage _messagePrefab;

    private const int MaxMessagesInList = 20;
    private List<ChatMessage> _messages;

    private const float MinIntervalBetweenChatMessages = 1f;
    private float _clientSendTimer;

    private void Start()
    {
        _messages = new List<ChatMessage>();
    }

    private void Update()
    {
        _clientSendTimer += Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Return))
        {
            if(_clientInput.text.Length > 0 && _clientSendTimer > MinIntervalBetweenChatMessages)
            {
                SendMessage();
                _clientInput.DeactivateInputField(clearSelection: true);
            }

            else
            {
                _clientInput.Select();
                _clientInput.ActivateInputField();
            }
        }
    }

    public void SendMessage()
    {
        string message = _clientInput.text;
        _clientInput.text = "";

        if (string.IsNullOrWhiteSpace(message)) return;
        _clientSendTimer = 0;
    }

    private void AddMessage(string message)
    {
        var chatMessage = Instantiate(_messagePrefab, _messageParent);
        chatMessage.SetMessage("Player", message);

        _messages.Add(chatMessage);

        if(_messages.Count > MaxMessagesInList)
        {
            Destroy(_messages[0]);
            _messages.RemoveAt(0);
        }
    }

    [ClientRpc]
    private void ReceiveChatMessageClientRpc(string message)
    {
        AddMessage(message);
    }

    [ServerRpc]
    private void SendMessageServerRpc(string message)
    {
        ReceiveChatMessageClientRpc(message);
    }
}
