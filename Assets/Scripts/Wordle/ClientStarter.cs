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

    // We allow the player to connect to a room using a join code with Relay.
    public async void StartClient(TMP_InputField joinCodeInput)
    {
        if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (IsRunning) return; // If the client is connected already, we don't connect again and stop the method.

        // We set the connection's settings using Relay.
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeInput.text);
        RelayServerData relayData = joinAllocation.ToRelayServerData("dtls");

        NetworkSettings settings = new NetworkSettings();
        settings.WithRelayParameters(ref relayData);

        //We connect the player.
        Driver = NetworkDriver.Create(settings);
        Connection = Driver.Connect(relayData.Endpoint);

        IsRunning = true;
        _clientUI.gameObject.SetActive(true);
    }
}
