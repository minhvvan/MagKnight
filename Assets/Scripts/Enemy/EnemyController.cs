using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController: MonoBehaviour
{
    public IReadOnlyList<Enemy> Enemies => _enemies;
    private List<Enemy> _enemies = new List<Enemy>();

    private bool NoEnemies => _enemies.Count <= 0;

    public Action OnEnemiesClear;

    private void Awake()
    {
        var enemies = GetComponentsInChildren<Enemy>().ToList();

        foreach (var enemy in enemies)
        {
            AddEnemy(enemy);
            enemy.OnDead += RemoveEnemy;
        }
    }

    private void AddEnemy(Enemy enemy)
    {
        _enemies.Add(enemy);
    }

    private void RemoveEnemy(Enemy enemy)
    {
        _enemies.Remove(enemy);
        if(NoEnemies) OnEnemiesClear?.Invoke();
    }

    public void ClearAllEnemies()
    {
        // 적이 없으면 바로 리턴
        if(_enemies.Count == 0) return;
    
        for(int i = _enemies.Count - 1; i >= 0; i--)
        {
            _enemies[i].gameObject.Destroy();
        }
    
        _enemies.Clear();
    }
}
