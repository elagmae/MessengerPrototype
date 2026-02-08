using System;
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
    [SerializeField]
    private UpdateGuessColors _guessColors;

    [SerializeField]
    private Canvas _clientUI;
    [SerializeField]
    private Canvas _wonUi;
    [field:SerializeField]
    public GameObject GuessPanel { get; private set; }

    private NetworkDriver driver;
    private NetworkConnection connection;

    private bool isRunning = false;
    public bool EndGame { get; private set; } = false;
    private int _currentTry = -1;
    private string _wordToGuess = "";

    async void Awake()
    {
        Application.runInBackground = true;
        await InitUnityServices();
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(_clientUI.gameObject);
    }

    private void DisplayUi()
    {
        _clientUI.gameObject.SetActive(true);
    }

    async Task InitUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously (CLIENT): " + AuthenticationService.Instance.PlayerId);
            }
        }

        catch (Exception _)
        {
        }

    }

    public async void StartClient(TMP_InputField joinCodeInput)
    {
        if (isRunning) return;

        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeInput.text);

        Debug.Log("CLIENT joined Relay.");

        RelayServerData relayData = joinAllocation.ToRelayServerData("dtls");

        NetworkSettings settings = new NetworkSettings();
        settings.WithRelayParameters(ref relayData);

        driver = NetworkDriver.Create(settings);

        connection = driver.Connect(relayData.Endpoint);

        isRunning = true;
        DisplayUi();
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
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("CLIENT connected to server !");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                byte msgType = stream.ReadByte();

                print(msgType);

                if (msgType == 1)
                {
                    _wordToGuess = stream.ReadFixedString128().ToString();
                }

                if (msgType == 2 || msgType == 3)
                {
                    string colors = stream.ReadFixedString128().ToString();
                    int line = stream.ReadInt();
                    _guessColors.UpdateColors(colors, _currentTry);
                    return;

                }

                if(msgType == 4)
                {
                    string wonMsg = stream.ReadFixedString128().ToString();
                    if (wonMsg == "Won")
                    {
                        _guessColors.UpdateColors("GGGGG", _currentTry);
                        Debug.Log("Won! ");
                        _wonUi.gameObject.SetActive(true);
                        EndGame = true;
                    }
                }
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Disconnected from server.");
                connection = default;
            }
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

        for (int i = 0; i < message.Length; i++)
        {
            _guessColors.Lines[_currentTry].Tmps[i].text = message[i].ToString();
        }

        driver.EndSend(writer);

        Debug.Log("CLIENT sent: " + message);
    }

    private void OnDestroy()
    {
        if (driver.IsCreated)
            driver.Dispose();
    }
}
