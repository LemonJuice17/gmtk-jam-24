using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AssetRandomiser))]
public class AssetRandomiserEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AssetRandomiser assetRandomiser = (AssetRandomiser)target;

        if (GUILayout.Button("Randomise Asset"))
        {
            assetRandomiser.Randomise();
        }

        if (GUILayout.Button("Set Initial Values"))
        {
            assetRandomiser.SetInitialValues();
        }
    }
}
