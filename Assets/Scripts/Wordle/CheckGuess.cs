using TMPro;
using Unity.Collections;
using UnityEngine;

public class CheckGuess : MonoBehaviour
{
    [field: SerializeField] public UpdateGuessColors GuessColors { get; private set; }

    public int CurrentTry { get; private set; } = -1;

    [SerializeField] private TMP_InputField _guessInput;

    private ClientStarter _client;
    private ClientUpdater _updater;

    private void Awake()
    {
        TryGetComponent(out _client);
        TryGetComponent(out _updater);
    }

    private void Update()
    {
        if (!Input.GetKeyUp(KeyCode.Return) || !_updater.GuessPanel.activeInHierarchy || _updater.EndGame) return;
        if (_guessInput.text.Length < 5) return;

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
