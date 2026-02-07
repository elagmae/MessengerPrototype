using System;
using System.Threading.Tasks;
using TMPro;
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
    public string JoinCode { get; private set; }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay(int maxConnections)
    {
        try
        {
            await Task.Yield();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = allocation.ToRelayServerData("wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async void JoinRelay(TMP_InputField joinCodeInput)
    {
        try
        {
            await Task.Yield();

            JoinCode = joinCodeInput.text.Trim();

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(JoinCode);
            RelayServerData relayServerData = joinAllocation.ToRelayServerData("wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }

        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
