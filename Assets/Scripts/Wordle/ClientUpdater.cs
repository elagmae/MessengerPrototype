using System.Threading.Tasks;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Services.Core;
using UnityEngine;

public class ClientUpdater : MonoBehaviour
{
    [field: SerializeField] public GameObject GuessPanel { get; private set; }

    public bool EndGame { get; set; } = false;

    private CheckGuess _checker;
    private ClientStarter _starter;
    private string _wordToGuess = "";

    async void Awake()
    {
        await Task.Yield();

        Application.runInBackground = true;
        TryGetComponent(out _checker);
        TryGetComponent(out _starter);

        await UnityServices.InitializeAsync();

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (_starter == null || !_starter.IsRunning || EndGame) return;

        if(_wordToGuess != "") GuessPanel.SetActive(true);

        _starter.Driver.ScheduleUpdate().Complete();

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = _starter.Connection.PopEvent(_starter.Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Data)
            {
                byte msgType = stream.ReadByte();

                // 1 = Word to guess from the host.
                if (msgType == 1) _wordToGuess = stream.ReadFixedString128().ToString();

                // 2 = Response from the client's guess (if the letters are green, yellor, or red).
                if (msgType == 2) _checker.GuessColors.UpdateColors(stream.ReadFixedString128().ToString(), _checker.CurrentTry);
            }

            else if (cmd == NetworkEvent.Type.Disconnect) _starter.Connection = default;
        }
    }

    private void OnDestroy()
    {
        if (_starter.Driver.IsCreated) _starter.Driver.Dispose();
    }
}