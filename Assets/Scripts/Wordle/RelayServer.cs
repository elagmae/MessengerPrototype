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
using UnityEngine.SceneManagement;

public class RelayServer : MonoBehaviour
{
    [SerializeField]
    private Canvas _hostUi;
    [SerializeField]
    private UpdateGuessColors _guessColors;

    private NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    private AsyncOperation _loadOperation = new();
    private int _currentTry = -1;
    private string _wordToGuess;

    public TextMeshProUGUI joinCode;

    private bool isRunning = false;

    async void Awake()
    {
        await InitUnityServices();
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(_hostUi.gameObject);
    }

    private void DisplayUi()
    {
        _hostUi.gameObject.SetActive(true);
    }

    async Task InitUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously (SERVER): " + AuthenticationService.Instance.PlayerId);
            }
        }

        catch (Exception _)
        {
        }
    }

    public async void StartServer()
    {
        if (isRunning) return;

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
        joinCode.text = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        Debug.Log("SERVER started. JOIN CODE = " + joinCode);

        RelayServerData relayData = allocation.ToRelayServerData("dtls");

        NetworkSettings settings = new NetworkSettings();
        settings.WithRelayParameters(ref relayData);

        driver = NetworkDriver.Create(settings);

        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);

        driver.Bind(NetworkEndpoint.AnyIpv4);
        driver.Listen();

        isRunning = true;

        DisplayUi();
    }

    void Update()
    {
        if (!isRunning) return;

        driver.ScheduleUpdate().Complete();

        CleanupConnections();
        AcceptConnections();
        ReceiveMessages();
    }

    void CleanupConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                i--;
            }
        }
    }

    void AcceptConnections()
    {
        NetworkConnection conn;
        while ((conn = driver.Accept()) != default)
        {
            connections.Add(conn);
        }
    }

    void ReceiveMessages()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;

            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    byte msgType = stream.ReadByte();

                    if (msgType == 1)
                    {
                        FixedString128Bytes msg = stream.ReadFixedString128();
                        ShowColors(msg.ToString());
                        return;
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    connections[i] = default;
                }
            }
        }
    }

    void ShowColors(string word)
    {
        _currentTry++;
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated) continue;

            DataStreamWriter writer;
            driver.BeginSend(connections[i], out writer);

            if (word == _wordToGuess)
            {
                writer.WriteByte(4);
                writer.WriteFixedString128("Won");
            }

            string colors = "";
            foreach (char c in word)
            {
                if (c == _wordToGuess[word.IndexOf(c)]) colors += "G";
                else if (_wordToGuess.Contains(c)) colors += "Y";
                else if (!_wordToGuess.Contains(c)) colors += "R";
            }

            for (int j = 0; j < word.Length;j++)
            {
                _guessColors.Lines[_currentTry].Tmps[j].text = word[j].ToString();
            }

            writer.WriteByte(2);
            writer.WriteFixedString128(colors);

            writer.WriteByte(3);
            writer.WriteInt(_currentTry);

            _guessColors.UpdateColors(colors, _currentTry);

            driver.EndSend(writer);
        }
    }

    public void SendGuess(TMP_InputField input)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated) continue;

            DataStreamWriter writer;
            driver.BeginSend(connections[i], out writer);

            writer.WriteByte(1);
            writer.WriteFixedString128(input.text);

            driver.EndSend(writer);
            _wordToGuess = input.text;
        }
    }

    private void OnDestroy()
    {
        if (driver.IsCreated)
            driver.Dispose();

        if (connections.IsCreated)
            connections.Dispose();
    }
}
