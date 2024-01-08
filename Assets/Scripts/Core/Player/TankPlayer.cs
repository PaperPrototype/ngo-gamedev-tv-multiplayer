using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using Unity.Collections;
using System;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [field: SerializeField] public Health Health { get; private set; }

    [Header("Settings")]
    [SerializeField] private int cameraPriorityIfOwner = 20;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        // TODO this will not work for dedicated servers
        if (IsServer)
        {
            if (HostSingleton.Instance.GameManager.NetworkServer.TryGetUserDataByClientId(OwnerClientId, out UserData userData))
            {
                PlayerName.Value = userData.username;
            }

            OnPlayerSpawned?.Invoke(this);
        }

        if (IsOwner)
        {
            virtualCamera.Priority = cameraPriorityIfOwner;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
