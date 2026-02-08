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
    [SerializeField] private UpdateGuessColors _guessColors;

    private NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    private int _currentTry = -1;
    private string _wordToGuess;

    private bool isRunning = false;
    private bool _hasPendingUIUpdate;
    private string _pendingWord;
    private string _pendingColors;
    private int _pendingTry;

    async void Awake()
    {
        await Task.Yield();

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

        driver = NetworkDriver.Create(settings);

        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);

        driver.Bind(NetworkEndpoint.AnyIpv4);
        driver.Listen();

        isRunning = true;

        _hostUi.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isRunning) return;

        driver.ScheduleUpdate().Complete();

        CleanupConnections();
        AcceptConnections();
        ReceiveMessages();

        if (_hasPendingUIUpdate)
        {
            _hasPendingUIUpdate = false;

            for (int j = 0; j < _pendingWord.Length; j++) _guessColors.Lines[_pendingTry].Tmps[j].text = _pendingWord[j].ToString();

            _guessColors.UpdateColors(_pendingColors, _pendingTry);
        }
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
            _waitPanel.SetActive(false);
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
                    if (msgType == 1) ShowColors(stream.ReadFixedString128().ToString());
                }

                else if (cmd == NetworkEvent.Type.Disconnect) connections[i] = default;
            }
        }
    }

    void ShowColors(string word)
    {
        _currentTry++;

        string colors = "";
        foreach (char c in word)
        {
            if (c == _wordToGuess[word.IndexOf(c)]) colors += "G";
            else if (_wordToGuess.Contains(c)) colors += "Y";
            else colors += "R";
        }

        _pendingWord = word;
        _pendingColors = colors;
        _pendingTry = _currentTry;
        _hasPendingUIUpdate = true;

        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated) continue;

            DataStreamWriter writer;
            driver.BeginSend(connections[i], out writer);

            writer.WriteByte(2);
            writer.WriteFixedString128(colors);

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
        if (driver.IsCreated) driver.Dispose();
        if (connections.IsCreated) connections.Dispose();
    }
}
