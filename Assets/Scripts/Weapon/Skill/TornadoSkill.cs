using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoSkill : Skill
{
    [SerializeField] private float moveSpeed;

    new void Start()
    {
        base.Start();
        Destroy(gameObject, 5f);
    }

    void FixedUpdate()
    {
        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime));
    }
}
