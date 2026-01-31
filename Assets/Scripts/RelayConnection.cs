using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine; 

public class RelayConnection : MonoBehaviour
{
    private async void Start()
    {
        // Initialize UnityServices
        await UnityServices.InitializeAsync();

        // If not signed in yet, sign it anonmously
        if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    /// <summary>
    /// Creates a Relay Server with a number of max connections, and updates the _joinCode.
    /// </summary>
    /// <param name="maxConnections"></param>
    /// <returns></returns>
    [ContextMenu("CreateRelay")]
    public async void CreateRelay(int maxConnections)
    {
        try
        {
            await Task.Yield();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

            // Sets _joinCode from allocation ID
            GameManager.Instance.CodeDisplay.text = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Sets connection type to wss (web socket) and creates server data from allocation
            var connectionType = "wss";
            RelayServerData relayServerData = allocation.ToRelayServerData(connectionType);

            // Sets server parameters
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Starts hosting on Relay Server
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// Joins a Relay Server from a joinCode.
    /// </summary>
    /// <param name="joinCode"></param>
    [ContextMenu("JoinRelay")]
    public async void JoinRelay(string joinCode)
    {
        try
        {
            await Task.Yield();

            Debug.Log("Joining Relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Sets connection type to wss (web socket) and creates server data from allocation
            var connectionType = "wss";
            RelayServerData relayServerData = joinAllocation.ToRelayServerData(connectionType);

            // Sets server parameters
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Joins Relay Server as client
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex);
        }
    }
}
