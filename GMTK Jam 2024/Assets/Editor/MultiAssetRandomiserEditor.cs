using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MultiAssetRandomiser))]
public class MultiAssetRandomiserEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MultiAssetRandomiser multiAssetRandomiser = (MultiAssetRandomiser)target;

        if (GUILayout.Button("Randomise Assets"))
        {
            multiAssetRandomiser.RandomiseAll();
        }

        if (GUILayout.Button("Set Initial Values"))
        {
            multiAssetRandomiser.SetInitialValuesForAll();
        }
    }
}
