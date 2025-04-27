using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class Skill : MonoBehaviour
{
    [SerializeField] float damageInterval = 0.5f;
    [SerializeField] GameplayEffect _damageEffect;
    [SerializeField] GameplayEffect _resEffect;
    //[SerializeField] LayerMask _layerMask;

    [SerializeField] private SerializedDictionary<Collider, float> damageTimers;

    void Start()
    {
        //_layerMask = LayerMask.GetMask("Enemy");
    }
    void FixedUpdate()
    {
        //transform.position += Vector3.forward * (Time.deltaTime * 10);
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (damageTimers.TryAdd(other, 0f))
            {
                var enemyASC = other.GetComponent<Enemy>().blackboard.abilitySystem;
                enemyASC.ApplyEffect(_damageEffect);
                enemyASC.ApplyEffect(_resEffect);
            }

            // 새로 들어온 적이면 타이머 0으로 시작
            damageTimers[other] += Time.deltaTime;

            if (damageTimers[other] >= damageInterval)
            {
                damageTimers[other] = 0f;
                var enemyASC = other.GetComponent<Enemy>().blackboard.abilitySystem;
                enemyASC.ApplyEffect(_damageEffect);
                enemyASC.ApplyEffect(_resEffect);     
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (damageTimers.ContainsKey(other))
        {
            damageTimers.Remove(other); // 영역을 벗어나면 타이머 삭제
        }
    }
}
