using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AssetSplotch : MonoBehaviour
{
    [SerializeField] GameObject[] assets;
    [SerializeField] Transform[] spawnpoints;

    private void Awake()
    {
        for (int i = 0; i < spawnpoints.Length; i++)
        {
            int assetIndex = Random.Range(0, assets.Length);
            Instantiate(assets[assetIndex], spawnpoints[i]);
        }
    }
}
