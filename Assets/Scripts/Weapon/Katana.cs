
using UnityEngine;

public class Katana: BaseWeapon
{
    public override void AttackStart()
    {
        base.AttackStart();
    }

    public override void AttackEnd()
    {
        base.AttackEnd();

    }

    protected override void OnHit(HitInfo hitInfo)
    {
        Debug.Log($"{hitInfo.hit.collider.name} : Hit at {hitInfo.time:F1}s");
    }
}
