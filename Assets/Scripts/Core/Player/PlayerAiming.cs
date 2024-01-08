using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turretTransform;

    private void LateUpdate()
    {
        if (!IsOwner) return;

        Vector2 aimWorldPosition = Camera.main.ScreenToWorldPoint(inputReader.AimPosition);

        turretTransform.up = aimWorldPosition - (Vector2)turretTransform.position;
    }
}
