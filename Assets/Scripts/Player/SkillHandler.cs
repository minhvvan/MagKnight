using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHandler : MonoBehaviour
{
    [SerializeField] private GameObject FieldSkillObj;
    [SerializeField] private GameObject TornadoObj;
    [SerializeField] private GameObject ProjectilObj;
    
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    public void UseSkill(int skillNumber)
    {
        switch (skillNumber)
        {
            case 1:
                animator.SetInteger("SkillIndex", 1);
                animator.SetTrigger("Skill");
                break;
            case 2:
                animator.SetInteger("SkillIndex", 2);
                animator.SetTrigger("Skill");
                break;
            case 3:
                //animator.SetTrigger("Skill3");
                break;
            default:
                Debug.LogWarning("Invalid Skill Number");
                break;
        }
    }

    // 애니메이션 이벤트로 호출되는 함수들
    public void OnFieldSpawn()
    {
        Instantiate(FieldSkillObj, transform.position, transform.rotation);
    }

    public void OnTornadoSpawn()
    {
        Instantiate(TornadoObj, transform.position, transform.rotation);
        // 토네이도 생성 코드
    }

    public void OnProjectileSpawn()
    {
        Debug.Log("발사체 생성!");
        // 발사체 생성 코드
    }
}
