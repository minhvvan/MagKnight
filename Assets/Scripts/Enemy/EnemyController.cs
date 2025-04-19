using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class EnemyController
{
    public static IReadOnlyList<Enemy> Enemies => _enemies;
    private static List<Enemy> _enemies = new List<Enemy>();

    public static bool NoEnemies => _enemies.Count <= 0;
    
    public static void AddEnemy(Enemy enemy)
    {
        _enemies.Add(enemy);
    }

    public static void RemoveEnemy(Enemy enemy)
    {
        _enemies.Remove(enemy);
    }
}
