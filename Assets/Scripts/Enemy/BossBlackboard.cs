using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBlackboard : EnemyBlackboard
{
    [HideInInspector] public int phase;
    [HideInInspector] public Texture2D phase2Texture;
    
    public override void Initialize()
    {
        base.Initialize(); // 공통 초기화 수행

        phase = 1;

        phase2Texture = new Texture2D(1, 1);
        phase2Texture.SetPixel(1, 1, Color.red);
        phase2Texture.Apply();

        ai = new BossAI(_enemy);
        action = new BossAction(_enemy);
        

    }
}
