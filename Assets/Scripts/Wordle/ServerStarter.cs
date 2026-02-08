using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class ServerStarter : MonoBehaviour
{
    [field: SerializeField] public TextMeshProUGUI JoinCode { get; private set; }

    public NativeList<NetworkConnection> Connections;
    public NetworkDriver Driver { get; set; }
    public bool IsRunning { get; private set; }

    [SerializeField] private Canvas _hostUi;

    private void Awake() => DontDestroyOnLoad(_hostUi.gameObject);

    public async void StartServer()
    {
        if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
        if (IsRunning) return;

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
        JoinCode.text = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        RelayServerData relayData = allocation.ToRelayServerData("dtls");
        NetworkSettings settings = new NetworkSettings();
        settings.WithRelayParameters(ref relayData);

        Driver = NetworkDriver.Create(settings);
        Connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);

        Driver.Bind(NetworkEndpoint.AnyIpv4);
        Driver.Listen();

        IsRunning = true;
        _hostUi.gameObject.SetActive(true);
    }
}
