using TMPro;
using Unity.Collections;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class CheckGuess : MonoBehaviour
{
    [field: SerializeField] public UpdateGuessColors GuessColors { get; private set; }
    public int CurrentTry { get; private set; } = -1;

    [SerializeField] private TMP_InputField _guessInput;

    private RelayClient _client;

    private void Awake()
    {
        TryGetComponent(out _client);
    }

    private void Update()
    {
        if (!Input.GetKeyUp(KeyCode.Return) || !_client.GuessPanel.activeInHierarchy || _client.EndGame) return;

        if (_guessInput.text.Length < 5)
        {
            Debug.Log("Not enough letters. Should be a five letter word !");
            return;
        }

        SendMessageToServer(_guessInput.text);
    }

    public void SendMessageToServer(string message)
    {
        if (!_client.Connection.IsCreated) return;

        CurrentTry++;

        DataStreamWriter writer;
        _client.Driver.BeginSend(_client.Connection, out writer);

        writer.WriteByte(1);
        writer.WriteFixedString128(message);

        for (int i = 0; i < message.Length; i++) GuessColors.Lines[CurrentTry].Tmps[i].text = message[i].ToString();

        _client.Driver.EndSend(writer);
        _guessInput.text = "";
        _guessInput.ActivateInputField();
    }
}
