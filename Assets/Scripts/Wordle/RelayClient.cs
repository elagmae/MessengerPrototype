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

    public bool EndGame { get; set; } = false;

    [SerializeField] private UpdateGuessColors _guessColors;
    [SerializeField] private Canvas _clientUI;

    private NetworkDriver driver;
    private NetworkConnection connection;

    private bool isRunning = false;
    private int _currentTry = -1;
    private string _wordToGuess = "";

    async void Awake()
    {
        await Task.Yield();

        Application.runInBackground = true;

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

        driver = NetworkDriver.Create(settings);
        connection = driver.Connect(relayData.Endpoint);

        isRunning = true;
        _clientUI.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isRunning || EndGame) return;

        if(_wordToGuess != "") GuessPanel.SetActive(true);

        driver.ScheduleUpdate().Complete();

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Data)
            {
                byte msgType = stream.ReadByte();

                if (msgType == 1) _wordToGuess = stream.ReadFixedString128().ToString();
                if (msgType == 2) _guessColors.UpdateColors(stream.ReadFixedString128().ToString(), _currentTry);
            }

            else if (cmd == NetworkEvent.Type.Disconnect) connection = default;
        }
    }

    public void SendMessageToServer(string message)
    {
        if (!connection.IsCreated) return;

        _currentTry++;

        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);

        writer.WriteByte(1);
        writer.WriteFixedString128(message);

        for (int i = 0; i < message.Length; i++) _guessColors.Lines[_currentTry].Tmps[i].text = message[i].ToString();

        driver.EndSend(writer);
    }

    private void OnDestroy()
    {
        if (driver.IsCreated) driver.Dispose();
    }
}
