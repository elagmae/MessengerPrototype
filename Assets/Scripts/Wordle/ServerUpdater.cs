using System.Threading.Tasks;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Services.Core;
using UnityEngine;

public class ServerUpdater : MonoBehaviour
{
    public string WordToGuess { get; set; }

    [SerializeField] private GameObject _waitPanel;

    private GetColorsHandler _colorsHandler;
    private ServerStarter _hostStarter;

    async void Awake()
    {
        await Task.Yield();

        TryGetComponent(out _colorsHandler);
        TryGetComponent(out _hostStarter);

        Application.runInBackground = true;
        await UnityServices.InitializeAsync();

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (_hostStarter == null || !_hostStarter.IsRunning) return; // If a host is connected.
        _hostStarter.Driver.ScheduleUpdate().Complete();

        for (int i = 0; i < _hostStarter.Connections.Length; i++)
        {
            if (!_hostStarter.Connections[i].IsCreated)
            {
                _hostStarter.Connections.RemoveAtSwapBack(i);
                i--;
            }

            DataStreamReader stream;
            NetworkEvent.Type cmd;

            while ((cmd = _hostStarter.Driver.PopEventForConnection(_hostStarter.Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    byte msgType = stream.ReadByte();

                    // Show right colors when a client sends a guess, comapring it to the word chosen by the host.
                    if (msgType == 1) _colorsHandler.ShowColors(stream.ReadFixedString128().ToString(), WordToGuess);
                }

                else if (cmd == NetworkEvent.Type.Disconnect) _hostStarter.Connections[i] = default;
            }
        }

        NetworkConnection conn;
        while ((conn = _hostStarter.Driver.Accept()) != default)
        {
            _hostStarter.Connections.Add(conn);
            _waitPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_hostStarter.Driver.IsCreated) _hostStarter.Driver.Dispose();
        if (_hostStarter.Connections.IsCreated) _hostStarter.Connections.Dispose();
    }
}
