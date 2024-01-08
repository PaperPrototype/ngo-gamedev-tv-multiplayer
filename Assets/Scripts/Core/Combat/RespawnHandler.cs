using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Mathematics;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
        foreach (TankPlayer player in players)
        {
            HandlePlayerSpawned(player);
        }

        TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }



    private void HandlePlayerSpawned(TankPlayer player)
    {
        player.Health.OnDie += (_health) => HandlePlayerDie(player);
    }
    private void HandlePlayerDespawned(TankPlayer player)
    {
        player.Health.OnDie -= (_health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(TankPlayer player)
    {
        Destroy(player.gameObject);

        StartCoroutine(RespawnPlayerNextFrame(player.OwnerClientId));
    }

    private IEnumerator RespawnPlayerNextFrame(ulong ownerClientId)
    {
        yield return null; // wait until the next frame

        NetworkObject playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPosition(), Quaternion.identity);

        // assign to player that died
        playerInstance.SpawnAsPlayerObject(ownerClientId);
    }
}