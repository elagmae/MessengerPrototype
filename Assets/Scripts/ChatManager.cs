using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : NetworkManager
{
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

        if (GUILayout.Button("Host", button)) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client", button)) NetworkManager.Singleton.StartClient();
    }

    private void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }
}
