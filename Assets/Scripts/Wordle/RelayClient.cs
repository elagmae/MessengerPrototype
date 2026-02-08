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

public class RelayClient : MonoBehaviour
{
    [field: SerializeField] public GameObject GuessPanel { get; private set; }

    public NetworkDriver Driver {get;private set;}
    public NetworkConnection Connection => connection;
    public bool EndGame { get; set; } = false;

    [SerializeField] private Canvas _clientUI;

    private CheckGuess _checker;
    private NetworkConnection connection;

    private bool isRunning = false;
    private string _wordToGuess = "";

    async void Awake()
    {
        await Task.Yield();

        Application.runInBackground = true;
        TryGetComponent(out _checker);

        await UnityServices.InitializeAsync();

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(_clientUI.gameObject);
    }

    public async void StartClient(TMP_InputField joinCodeInput)
    {
        if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (isRunning) return;

        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeInput.text);
        RelayServerData relayData = joinAllocation.ToRelayServerData("dtls");

        NetworkSettings settings = new NetworkSettings();
        settings.WithRelayParameters(ref relayData);

        Driver = NetworkDriver.Create(settings);
        connection = Driver.Connect(relayData.Endpoint);

        isRunning = true;
        _clientUI.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isRunning || EndGame) return;

        if(_wordToGuess != "") GuessPanel.SetActive(true);

        Driver.ScheduleUpdate().Complete();

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = connection.PopEvent(Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Data)
            {
                byte msgType = stream.ReadByte();

                if (msgType == 1) _wordToGuess = stream.ReadFixedString128().ToString();
                if (msgType == 2) _checker.GuessColors.UpdateColors(stream.ReadFixedString128().ToString(), _checker.CurrentTry);
            }

            else if (cmd == NetworkEvent.Type.Disconnect) connection = default;
        }
    }

    private void OnDestroy()
    {
        if (Driver.IsCreated) Driver.Dispose();
    }
}