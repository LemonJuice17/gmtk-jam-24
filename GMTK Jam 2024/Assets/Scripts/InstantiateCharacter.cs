using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateCharacter : MonoBehaviour
{
    public GameObject CharacterPrefab;

    public GameObject InstantiateMe() => InstantiateMe(transform.position, Quaternion.identity);
    public GameObject InstantiateMe(Vector3 position) => InstantiateMe(position, Quaternion.identity);
    public GameObject InstantiateMe(Vector3 position, Quaternion rotation)
    {
        Instantiate(GameManager.instance.InstantiationPoofEffect, position, Quaternion.identity);
        return Instantiate(CharacterPrefab, position, rotation); 
    }
    public static GameObject InstantiateCharacterStatic(GameObject characterPrefab, Vector3 position, Quaternion rotation)
    {
        Instantiate(GameManager.instance.InstantiationPoofEffect, position, Quaternion.identity);
        return Instantiate(characterPrefab, position, rotation);
    }
}