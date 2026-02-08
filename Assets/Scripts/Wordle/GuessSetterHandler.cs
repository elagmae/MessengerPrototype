using TMPro;
using Unity.Collections;
using UnityEngine;

public class GuessSetterHandler : MonoBehaviour
{
    private ServerStarter _server;
    private ServerUpdater _updater;

    private void Awake()
    {
        TryGetComponent(out _server);
        TryGetComponent(out _updater);
    }

    public void SendGuess(TMP_InputField input)
    {
        for (int i = 0; i < _server.Connections.Length; i++)
        {
            if (!_server.Connections[i].IsCreated) continue;

            DataStreamWriter writer;
            _server.Driver.BeginSend(_server.Connections[i], out writer);

            writer.WriteByte(1);
            writer.WriteFixedString128(input.text);

            _server.Driver.EndSend(writer);
            _updater.WordToGuess = input.text;
        }
    }
}
