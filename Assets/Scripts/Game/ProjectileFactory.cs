using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProjectileFactory
{
    public static Projectile Create(GameObject projectilePrefab, Vector3 position, Quaternion rotation, 
        ProjectileLaunchData launchData)
    {
        var obj = GameObject.Instantiate(projectilePrefab, position, rotation);
        var projectile = obj.GetComponent<Projectile>();
        projectile.Initialize(launchData);
        return projectile;
    }
}
