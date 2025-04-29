using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class FieldSkill : Skill
{
    new void Start()
    {
        base.Start();
      
        Destroy(gameObject, 5f);
    }
}
