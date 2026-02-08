using TMPro;
using Unity.Collections;
using UnityEngine;

public class GetColorsHandler : MonoBehaviour
{
    [SerializeField] private UpdateGuessColors _guessColors;

    private RelayServer _server;
    private bool _hasPendingUIUpdate;
    private string _pendingWord;
    private string _pendingColors;
    private int _pendingTry;
    private int _currentTry = -1;

    private void Awake() => TryGetComponent(out _server);

    private void Update()
    {
        if (_hasPendingUIUpdate)
        {
            _hasPendingUIUpdate = false;
            for (int j = 0; j < _pendingWord.Length; j++) _guessColors.Lines[_pendingTry].Tmps[j].text = _pendingWord[j].ToString();
            _guessColors.UpdateColors(_pendingColors, _pendingTry);
        }
    }

    public void ShowColors(string word, string wordToGuess)
    {
        _currentTry++;

        string colors = "";
        foreach (char c in word)
        {
            if (c == wordToGuess[word.IndexOf(c)]) colors += "G";
            else if (wordToGuess.Contains(c)) colors += "Y";
            else colors += "R";
        }

        _pendingWord = word;
        _pendingColors = colors;
        _pendingTry = _currentTry;
        _hasPendingUIUpdate = true;

        for (int i = 0; i < _server.Connections.Length; i++)
        {
            if (!_server.Connections[i].IsCreated) continue;

            DataStreamWriter writer;
            _server.Driver.BeginSend(_server.Connections[i], out writer);

            writer.WriteByte(2);
            writer.WriteFixedString128(colors);

            _server.Driver.EndSend(writer);
        }
    }
}
