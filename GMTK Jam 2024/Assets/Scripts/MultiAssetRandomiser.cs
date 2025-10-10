using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiAssetRandomiser : MonoBehaviour
{
    List<AssetRandomiser> assetRandomisers = new List<AssetRandomiser>();

    public void RandomiseAll()
    {
        foreach (AssetRandomiser assetRandomiser in GetComponentsInChildren<AssetRandomiser>()) 
        { 
            assetRandomiser.Randomise(); 
        }
    }

    public void SetInitialValuesForAll()
    {
        foreach (AssetRandomiser assetRandomiser in GetComponentsInChildren<AssetRandomiser>())
        {
            assetRandomiser.SetInitialValues();
        }
    }
}
