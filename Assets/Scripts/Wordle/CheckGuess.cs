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
        // If the player validates their word using the enter key, and if their word is long enough :
        if (!Input.GetKeyUp(KeyCode.Return) || !_updater.GuessPanel.activeInHierarchy || _updater.EndGame) return;
        if (_guessInput.text.Length < 5) return;

        SendMessageToServer(_guessInput.text); // We send it to the server for verification.
    }

    public void SendMessageToServer(string message)
    {
        if (!_client.Connection.IsCreated) return;

        CurrentTry++; // We update the line the player is writing on.

        if (CurrentTry > 4) return;

        // We send our word to the server.
        DataStreamWriter writer;
        _client.Driver.BeginSend(_client.Connection, out writer);

        writer.WriteByte(1);
        writer.WriteFixedString128(message); 

        // We display the word in the current line's blanks.
        for (int i = 0; i < message.Length; i++) GuessColors.Lines[CurrentTry].Tmps[i].text = message[i].ToString();

        _client.Driver.EndSend(writer);

        // We empty the input field and reactivate it, allowing the player to type another word immediatly.
        _guessInput.text = "";
        _guessInput.ActivateInputField();
    }
}
