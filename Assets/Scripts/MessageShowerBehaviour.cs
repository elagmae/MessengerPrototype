using Unity.Netcode;
using UnityEngine;

public class MessageShowerBehaviour : MonoBehaviour
{
    [SerializeField] private MessageReceiveBehaviour _receiverPrefab;
    private MessageSendingBehaviour _messageSender;

    private void Awake() => TryGetComponent(out _messageSender);
}
