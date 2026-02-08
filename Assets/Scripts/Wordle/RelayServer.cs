using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayServer : MonoBehaviour
{
    [field: SerializeField] public TextMeshProUGUI JoinCode { get; private set; }

    [SerializeField] private Canvas _hostUi;
    [SerializeField] private GameObject _waitPanel;

    public NetworkDriver Driver { get; set; }
    public NativeList<NetworkConnection> Connections => connections;

    private NativeList<NetworkConnection> connections;
    private GetColorsHandler _colorsHandler;
    private string _wordToGuess;
    private bool isRunning = false;

    async void Awake()
    {
        await Task.Yield();

        TryGetComponent(out _colorsHandler);

        Application.runInBackground = true;
        await UnityServices.InitializeAsync();

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(_hostUi.gameObject);
    }

    public async void StartServer()
    {
        if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
        if (isRunning) return;

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
        JoinCode.text = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        RelayServerData relayData = allocation.ToRelayServerData("dtls");
        NetworkSettings settings = new NetworkSettings();
        settings.WithRelayParameters(ref relayData);

        Driver = NetworkDriver.Create(settings);
        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);

        Driver.Bind(NetworkEndpoint.AnyIpv4);
        Driver.Listen();

        isRunning = true;
        _hostUi.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isRunning) return;
        Driver.ScheduleUpdate().Complete();

        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                i--;
            }
        }

        NetworkConnection conn;
        while ((conn = Driver.Accept()) != default)
        {
            connections.Add(conn);
            _waitPanel.SetActive(false);
        }

        for (int i = 0; i < connections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;

            while ((cmd = Driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    byte msgType = stream.ReadByte();
                    if (msgType == 1) _colorsHandler.ShowColors(stream.ReadFixedString128().ToString(), _wordToGuess);
                }

                else if (cmd == NetworkEvent.Type.Disconnect) connections[i] = default;
            }
        }
    }

    public void SendGuess(TMP_InputField input)
    {
        for (int i = 0; i < Connections.Length; i++)
        {
            if (!Connections[i].IsCreated) continue;

            DataStreamWriter writer;
            Driver.BeginSend(Connections[i], out writer);

            writer.WriteByte(1);
            writer.WriteFixedString128(input.text);

            Driver.EndSend(writer);
            _wordToGuess = input.text;
        }
    }

    private void OnDestroy()
    {
        if (Driver.IsCreated) Driver.Dispose();
        if (Connections.IsCreated) Connections.Dispose();
    }
}
