using TMPro;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class ClientStarter : MonoBehaviour
{
    public bool IsRunning { get; private set; } = false;
    public NetworkDriver Driver { get; private set; }
    public NetworkConnection Connection;

    [SerializeField] private Canvas _clientUI;

    private void Awake() => DontDestroyOnLoad(_clientUI.gameObject);

    public async void StartClient(TMP_InputField joinCodeInput)
    {
        if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (IsRunning) return;

        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeInput.text);
        RelayServerData relayData = joinAllocation.ToRelayServerData("dtls");

        NetworkSettings settings = new NetworkSettings();
        settings.WithRelayParameters(ref relayData);

        Driver = NetworkDriver.Create(settings);
        Connection = Driver.Connect(relayData.Endpoint);

        IsRunning = true;
        _clientUI.gameObject.SetActive(true);
    }
}
