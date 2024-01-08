using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using Unity.Services.Authentication;

public class HostGameManager : IDisposable
{
    private Allocation allocation;
    private string joinCode;
    private string lobbyId;
    private const int MaxConnections = 20;
    private const string GameSceneName = "Game";

    public NetworkServer NetworkServer { get; private set; }

    public async Task StartHostAsync()
    {
        // create relay
        try
        {
            allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        // join code for players to join us through the relay
        try
        {
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        // set transport to use the relay
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        // create lobby
        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>()
                {
                    {
                        "JoinCode", new DataObject(
                            visibility: DataObject.VisibilityOptions.Member,
                            value: joinCode
                        )
                    }
                }
            };
            string playerName = PlayerPrefs.GetString(NameSelector.playerNameKey, "Missing name");
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(
                $"{playerName}'s Lobby",
                MaxConnections,
                lobbyOptions
            );

            lobbyId = lobby.Id;

            // start the coroutine in our parent class
            HostSingleton.Instance.StartCoroutine(KeepLobbyAlive(15));
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            return;
        }

        UserData userData = new UserData
        {
            username = PlayerPrefs.GetString(NameSelector.playerNameKey, "Missing name"),
            userAuthId = AuthenticationService.Instance.PlayerId,
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        // data to send to the server
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        // create the network server (that will receive the above payload)
        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        // start hosting server
        NetworkManager.Singleton.StartHost();

        // load the game scene
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    // called HeartbeatLobby in the course
    private IEnumerator KeepLobbyAlive(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public async void Dispose()
    {
        HostSingleton.Instance?.StopCoroutine(nameof(KeepLobbyAlive));

        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
            }

            lobbyId = string.Empty;
        }

        NetworkServer?.Dispose();
    }
}
