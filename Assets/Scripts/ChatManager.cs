using Unity.Netcode;
using UnityEngine;

public class ChatManager : NetworkManager
{
    private RelayConnection _relay;

    private void Awake() => TryGetComponent(out _relay);

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(100, 100, 500, 500));

        if (!IsClient && !IsServer && !IsHost) StartButtons();

        GUILayout.EndArea();
    }

    private void StartButtons()
    {
        GUIStyle button = new GUIStyle("Button")
        {
            fixedWidth = 400,
            fixedHeight = 100,
            fontSize = 30
        };

        if (GUILayout.Button("Host", button)) _relay.CreateRelay(2);

        if (GUILayout.Button("Client", button)) _relay.JoinRelay(GameManager.Instance.CodeField.text.Trim());
    }
}
