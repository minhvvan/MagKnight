using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Moon;
using UnityEngine;

public class RangedNormalAction : IEnemyAction
{
    // 공격, 자폭, 치료 등 각 enemy가 가지고 있는 행동양식 실행
    private Enemy _enemy;
    private EnemyBlackboard _blackboard;
    private float _startupDuration;
    
    private bool _shot = false;
    
    public RangedNormalAction(Enemy enemy)
    {
        _enemy = enemy;
        _blackboard = enemy.blackboard;
    }
    
    public void OnEnter()
    {
        _blackboard.actionRecoveryCancellation = new CancellationTokenSource();
        _enemy.SetAnimTrigger("Action");
        _startupDuration = 0f;
    }

    public void OnUpdate()
    {
        // 준비시간동안 player의 움직임을 따라가도록
        if (!_shot)
        {
            Vector3 dir = _blackboard.target.transform.position - _blackboard.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation, targetRot, Time.deltaTime * 20f);
            }
        }
        
        if (!_shot && _startupDuration > _blackboard.startupTime)
        {
            // agent의 desiredvelocity에 의해 이상한곳으로 rotation이 돌아가지 않도록 막음
            if(_enemy.IsAvailableTarget())
            {
                _enemy.Agent.SetDestination(_blackboard.target.transform.position);
            }
            
            _enemy.SetAnimTrigger("ActionRun");
            _shot = true;
            // _enemy.transform.LookAt(_blackboard.target.GetComponent<PlayerController>().cameraSettings.follow);
            ProjectileLaunchData launchData = new ProjectileLaunchData(_blackboard.target.GetComponent<Collider>());
            
            var rot = _blackboard.target.transform.position - _enemy.transform.position;
            Projectile projectile = ProjectileFactory.Create(_blackboard.projectilePrefab, _blackboard.muzzleTransform.position, Quaternion.LookRotation(rot), launchData);
            VFXManager.Instance.TriggerVFX(VFXType.ENEMY_MUZZLE_BURST, _blackboard.muzzleTransform.position, _blackboard.muzzleTransform.rotation);
            projectile.OnHit += _enemy.GiveDamageEffect;
        }
        _startupDuration += Time.deltaTime;
    }

    public void OnExit()
    {
        _blackboard.actionRecoveryCancellation.Cancel();
        _shot = false;
    }
}