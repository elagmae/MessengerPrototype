using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine; 

public class RelayConnectionHandler : MonoBehaviour
{
    public event Action<string> OnHosting;
    public event Action<string> OnJoining;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay(int maxConnections, string connectionType)
    {
        try
        {
            await Task.Yield();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            GameManager.Instance.CodeDisplay.text = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = allocation.ToRelayServerData(connectionType);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            OnHosting?.Invoke(GameManager.Instance.CodeDisplay.text);
        }

        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async void JoinRelay(string joinCode, string connectionType)
    {
        try
        {
            await Task.Yield();

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = joinAllocation.ToRelayServerData(connectionType);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }

        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }
}
