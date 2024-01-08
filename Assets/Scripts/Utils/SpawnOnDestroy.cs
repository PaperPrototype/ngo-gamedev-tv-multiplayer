using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject toSpawnPrefab;

    private void OnDestroy()
    {
        Instantiate(toSpawnPrefab, transform.position, Quaternion.identity);
    }
}