using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController
{
    public IReadOnlyList<Enemy> Enemies => _enemies;
    private List<Enemy> _enemies = new List<Enemy>();

    public bool NoEnemies => _enemies.Count <= 0;
    
    public void AddEnemy(Enemy enemy)
    {
        _enemies.Add(enemy);
    }

    public void RemoveEnemy(Enemy enemy)
    {
        _enemies.Remove(enemy);
    }
}
