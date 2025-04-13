using UnityEngine;


[CreateAssetMenu(fileName = "ChasingAI", menuName = "SO/Enemy/AI/ChasingAI")]
public class ChasingAISO : EnemyAI
{
    public override void ExecuteBehavior()
    {
        Debug.Log("Chasing AI");
    }
}
