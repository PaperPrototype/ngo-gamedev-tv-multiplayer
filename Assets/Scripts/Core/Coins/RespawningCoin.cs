using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class RespawningCoin : Coin
{
    public event Action<RespawningCoin> OnCollected;

    private Vector3 previousPosition;

    private void Update()
    {
        if (previousPosition != transform.position)
        {
            Show(true);
        }

        previousPosition = transform.position;
    }

    public override int Collect()
    {
        // if we are a client
        if (!IsServer)
        {
            Show(false);
            return 0;
        }
        // else we are the server...

        if (alreadyCollected) return 0;

        alreadyCollected = true;

        OnCollected?.Invoke(this);

        return coinValue;
    }

    public void Reset()
    {
        alreadyCollected = false;
    }
}
