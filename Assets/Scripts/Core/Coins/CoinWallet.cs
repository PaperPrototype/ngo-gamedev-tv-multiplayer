using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<Coin>(out Coin coin)) return;

        int coinValue = coin.Collect();

        if (!IsServer) return;

        TotalCoins.Value += coinValue;
    }

    public bool SpendCoins(int spendValue)
    {
        if (CanSpendCoins(spendValue))
        {
            TotalCoins.Value -= spendValue;
            return true;
        }

        return false;
    }

    public bool CanSpendCoins(int spendValue)
    {
        if (TotalCoins.Value - spendValue >= 0)
        {
            return true;
        }

        return false;
    }
}
