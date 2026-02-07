using System;
using System.Collections.Generic;
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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RelayClient : MonoBehaviour
{
    [SerializeField]
    private Canvas _clientUI;
    [field:SerializeField]
    public GameObject GuessPanel { get; private set; }
    [SerializeField]
    private List<Word> _lines = new();

    private NetworkDriver driver;
    private NetworkConnection connection;
    private AsyncOperation _loadOperation = new();

    private bool isRunning = false;
    public bool EndGame { get; private set; } = false;
    private int _currentTry = -1;

    [Serializable]
    public struct Word
    {
        public List<Image> Images;
        public List<TextMeshProUGUI> Tmps;
    }

    async void Awake()
    {
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

        connection = driver.Connect(NetworkEndpoint.AnyIpv4);

        isRunning = true;
        DisplayUi();
    }

    void Update()
    {
        if (!isRunning || EndGame) return;

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
                    FixedString128Bytes wordToGuess = stream.ReadFixedString128();
                    GuessPanel.SetActive(true);
                }

                if (msgType == 2 || msgType == 3)
                {
                    string colors = stream.ReadFixedString128().ToString();
                    int line = stream.ReadInt();
                    UpdateColors(colors);
                    return;

                }

                if(msgType == 4)
                {
                    string wonMsg = stream.ReadFixedString128().ToString();
                    if (wonMsg == "Won")
                    {
                        UpdateColors("GGGGG");
                        Debug.Log("Won! ");
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
            _lines[_currentTry].Tmps[i].text = message[i].ToString();
        }

        driver.EndSend(writer);

        Debug.Log("CLIENT sent: " + message);
    }

    private void OnDestroy()
    {
        if (driver.IsCreated)
            driver.Dispose();
    }

    private void UpdateColors(string colors)
    {
        print(colors + " on line " + _currentTry);
        for (int i = 0; i < colors.Length; i++)
        {
            Color color = Color.white;
            switch(colors[i])
            {
                case 'G':
                    color = Color.green;
                    break;

                case 'Y':
                    color = Color.yellow;
                    break;

                case 'R':
                    color = Color.red;
                    break;
            }

            _lines[_currentTry].Images[i].color = color;
        }

    }
}
