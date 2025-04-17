using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlaceEnemyOnScene))]
public class PlaceEnemyInEditor : Editor
{
    PlaceEnemyOnScene enemy;
    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            enemy = (PlaceEnemyOnScene)target;
            _ = enemy.UpdateClosestCounter();
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
        {
            enemy.Disabled();
        }
    }
}