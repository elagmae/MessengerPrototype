using TMPro;
using UnityEngine;

public class CheckGuess : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _guessInput;
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

        _client.SendMessageToServer(_guessInput.text);
    }
}
