using TMPro;
using Unity.Collections;
using UnityEngine;

public class GuessSetterHandler : MonoBehaviour
{
    private RelayServer _server;

    private void Awake() => TryGetComponent(out _server);

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
            _server.WordToGuess = input.text;
        }
    }
}
