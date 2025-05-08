
using System;
using hvvan;
using UnityEngine;
using Random = UnityEngine.Random;

public class Katana: BaseWeapon
{
    [SerializeField] private GameObject slashVFXPrefab;
    [SerializeField] private GameObject vfxSocket;
    [SerializeField] int skillIndex;
    WeaponTrail _weaponTrail;

    private GameObject _whirlwindVFX;

    void Start()
    {
        _weaponTrail = GetComponent<WeaponTrail>();
    }
    

    public override void AttackStart(int hitboxGroupId)
    {
        base.AttackStart(hitboxGroupId);

        //VFX 재생
        if (hitboxGroupId == 1)
        {
            var player = GameManager.Instance.Player;
            _whirlwindVFX = VFXManager.Instance.TriggerVFX(VFXType.WHIRLWIND_KATANA, player.transform.position);
        }
        else
        {
            VFXManager.Instance.TriggerVFX(slashVFXPrefab, vfxSocket.transform.position, vfxSocket.transform.rotation);
        }
        
        //SFX 재생
        var sfxRandomClip = AudioManager.Instance.GetRandomClip(AudioBase.SFX.Player.Attack.Swing);
        AudioManager.Instance.PlaySFX(sfxRandomClip);
    }

    public override void AttackEnd(int hitboxGroupId)
    {
        base.AttackEnd(hitboxGroupId);
        if (hitboxGroupId == 1)
        {
            if (_whirlwindVFX)
            {
                VFXManager.Instance.ReturnVFX(VFXType.WHIRLWIND_KATANA, _whirlwindVFX);
                _whirlwindVFX = null;
            }
        }
    }

    public override int OnSkill()
    {
        return skillIndex;
    }

    public override void OnNext(HitInfo hitInfo)
    {
        base.OnNext(hitInfo);
    }

    public override void OnError(Exception error)
    {
        Debug.LogError(error);
    }

    public override void OnCompleted()
    {
    }
    
    public override void ChangePolarity()
    {
        //TODO: 극성 스위칭 효과
        // 2초간 대쉬 쿨타임 없음
    }
}
