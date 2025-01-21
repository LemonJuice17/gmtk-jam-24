using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateCharacter : MonoBehaviour
{
    public GameObject CharacterPrefab;

    public void InstantiateMe() => InstantiateMe(transform.position, Quaternion.identity);
    public void InstantiateMe(Vector3 position) => InstantiateMe(position, Quaternion.identity);
    public void InstantiateMe(Vector3 position, Quaternion rotation)
    {
        Instantiate(GameManager.instance.InstantiationPoofEffect, position, Quaternion.identity);
        Instantiate(CharacterPrefab, position, rotation); 
    }
    public static GameObject InstantiateCharacterStatic(GameObject characterPrefab, Vector3 position, Quaternion rotation)
    {
        Instantiate(GameManager.instance.InstantiationPoofEffect, position, Quaternion.identity);
        return Instantiate(characterPrefab, position, rotation);
    }
}