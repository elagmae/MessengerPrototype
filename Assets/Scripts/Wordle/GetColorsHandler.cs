using Unity.Collections;
using UnityEngine;

public class GetColorsHandler : MonoBehaviour
{
    [SerializeField] private UpdateGuessColors _guessColors;

    private ServerStarter _server;
    private bool _hasPendingUIUpdate;
    private string _pendingWord;
    private string _pendingColors;
    private int _pendingTry;
    private int _currentTry = -1;

    private void Awake() => TryGetComponent(out _server);

    private void Update()
    {
        if (_hasPendingUIUpdate) // When UI needs an update, we update it.
        {
            _hasPendingUIUpdate = false;
            for (int j = 0; j < _pendingWord.Length; j++) _guessColors.Lines[_pendingTry].Tmps[j].text = _pendingWord[j].ToString();
            _guessColors.UpdateColors(_pendingColors, _pendingTry);
        }
    }

    public void ShowColors(string word, string wordToGuess)
    {
        _currentTry++;

        if (_currentTry > 4) return;

        string colors = "";
        for(int i  = 0; i < word.Length; i++)
        {
            if (wordToGuess[i] == word[i]) colors += "G"; // If the letter is well placed, it's green.
            else if (wordToGuess.Contains(word[i])) colors += "Y"; // If the word contains the letter, but it's wrongly placed, it's yellow.
            else colors += "R"; // If the letter isn't in the word, it's red.
        }

        _pendingWord = word;
        _pendingColors = colors;
        _pendingTry = _currentTry;
        _hasPendingUIUpdate = true;

        for (int i = 0; i < _server.Connections.Length; i++)
        {
            if (!_server.Connections[i].IsCreated) continue;

            // We send the answer to the client.
            DataStreamWriter writer;
            _server.Driver.BeginSend(_server.Connections[i], out writer);

            writer.WriteByte(2);
            writer.WriteFixedString128(colors);

            _server.Driver.EndSend(writer);
        }
    }
}
