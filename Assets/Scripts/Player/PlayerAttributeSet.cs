using System;
using Cysharp.Threading.Tasks;
using hvvan;
using Moon;
using Unity.VisualScripting;
using UnityEngine;

namespace Jun
{
    public class PlayerAttributeSet : AttributeSet
    {
        public Action OnDead;
        public Action<ExtraData> OnDamaged;
        public Action<ExtraData> OnImpulse;
        public Action OnMoveSpeedChanged;
        public Action OnAttackSpeedChanged;
        
        private string superArmor = "SuperArmor";
        private string invisibility = "Invincibility";
        private string parry = "Parry";
        private float maxAttackSpeed = 2f;
        private float minAttackSpeed = 0.5f;
        private float maxMoveSpeed = 2f;
        private float minMoveSpeed = 0.8f;
        private float maxDefense = 100f;
        
        protected override float PreAttributeChange(AttributeType type, float newValue)
        {
            float returnValue = newValue;

            if (type == AttributeType.MoveSpeed)
            {
                if (GetValue(AttributeType.MoveSpeed) + newValue <= minMoveSpeed)
                {
                    returnValue = minMoveSpeed - GetValue(AttributeType.MoveSpeed) ;
                }
                else if (GetValue(AttributeType.MoveSpeed) + newValue >= maxMoveSpeed)
                {
                    returnValue = maxMoveSpeed - GetValue(AttributeType.MoveSpeed);
                }
            }

            if (type == AttributeType.AttackSpeed)
            {
                if (GetValue(AttributeType.AttackSpeed) + newValue <= minAttackSpeed)
                {
                    returnValue = minAttackSpeed - GetValue(AttributeType.AttackSpeed);
                }
                else if (GetValue(AttributeType.AttackSpeed) + newValue >= maxAttackSpeed)
                {
                    returnValue = maxAttackSpeed - GetValue(AttributeType.AttackSpeed);
                }
            }
            
            if (type == AttributeType.Damage)
            {
                // 데미지가 음수면 0으로 처리
                returnValue = newValue < 0 ? 0 : newValue;
                
                // 무적효과
                if (tag != null)
                {
                    if (tag.Contains(invisibility) || tag.Contains(parry))
                        returnValue = 0;
                }
                
                // Defense% 만큼 데미지 감소
                returnValue = returnValue * (1 - GetValue(AttributeType.Defense)/ 100f);
            }

            if (type == AttributeType.Impulse)
            {
                returnValue = newValue < 0 ? 0 : newValue;

                // SuperArmor 상태
                if (tag != null)
                {
                    if (tag.Contains(superArmor) || tag.Contains(invisibility))
                    {
                        returnValue = 0;
                    }
                }
            }

            return returnValue;
        }

        protected override void PostGameplayEffectExecute(GameplayEffect effect)
        {
            // 최대체력 증가시 그만큼 HP도 증가
            if (effect.attributeType == AttributeType.MaxHP)
            {
                //SetValue(AttributeType.HP, GetValue(AttributeType.MaxHP));
            }

            // 체력 변경시 Clamp값으로
            if (effect.attributeType == AttributeType.HP)
            {
                SetValue(AttributeType.HP,Mathf.Clamp(GetValue(AttributeType.HP), 0f, GetValue(AttributeType.MaxHP)));
                
            }

            if (effect.attributeType == AttributeType.CriticalRate)
            {
                SetValue(AttributeType.CriticalRate, Mathf.Clamp(GetValue(AttributeType.CriticalRate), 0f, 1));
                
            }
            
            if (effect.attributeType == AttributeType.MoveSpeed)
            {
                OnMoveSpeedChanged?.Invoke();
            }

            if (effect.attributeType == AttributeType.AttackSpeed)
            {
                OnAttackSpeedChanged?.Invoke();
            }

            if (effect.attributeType == AttributeType.Defense)
            {
                SetValue(AttributeType.Defense, Mathf.Clamp(GetValue(AttributeType.Defense), float.NegativeInfinity, maxDefense));
            }
            
            if (effect.attributeType == AttributeType.Damage)
            {
                if (GetValue(AttributeType.Damage) == 0)
                {
                    return;
                }
                // 체력은 항상 데미지를 통해서만 접근
                // 현재 체력에서 데미지를 뺀 값을 적용해서 Hp업데이트, 단 0보다 작거나, MaxHp보다 크지 않게
                SetValue(AttributeType.HP, Mathf.Clamp(GetValue(AttributeType.HP) - effect.amount, 0f, GetValue(AttributeType.MaxHP)));
                // 예시 실드가 있다면?
                // SetValue(AttributeType.HP, GetValue(AttributeType.Shield) + GetValue(AttributeType.HP) - effect.amount);

                effect.extraData.damageType = effect.damageType;
                effect.extraData.finalAmount = effect.amount;

#if false //공격 효과
                //CameraShake.Shake(0.2f, 0.1f);
                CinemachineImpulseController.GenerateImpulse();
                Time.timeScale = 0.1f;
                UniTask.Delay(TimeSpan.FromMilliseconds(200f), DelayType.UnscaledDeltaTime).ContinueWith(() =>
                {
                    Time.timeScale = 1;
                });
#endif
                if (GetValue(AttributeType.HP) == 0)
                {
                    OnDead?.Invoke();
                }
                else
                {
                    //OnDamaged?.Invoke(effect.sourceTransform);
                    OnDamaged?.Invoke(effect.extraData);
                }
                // 메타 어트리뷰트는 적용 후 바로 0으로 값을 초기화하도록 설정
                SetValue(AttributeType.Damage, 0);
            }

            if (effect.attributeType == AttributeType.Impulse)
            {
                SetValue(AttributeType.Impulse, Mathf.Clamp(GetValue(AttributeType.Impulse) - GetValue(AttributeType.EndureImpulse), 0, 100));
             
                // 적용 후 충격량이 0일 때
                if(GetValue(AttributeType.Impulse) == 0)
                    return;
                
                if(effect.extraData.sourceTransform != null)
                    OnImpulse?.Invoke(effect.extraData);
                    
                SetValue(AttributeType.Impulse, 0);
            }

            // 최대체력 증가시 그만큼 HP도 증가
            if (effect.attributeType == AttributeType.MaxSkillGauge)
            {
                SetValue(AttributeType.SkillGauge, GetValue(AttributeType.MaxSkillGauge));
            }
            
            if (effect.attributeType == AttributeType.SkillGauge)
            {
                SetValue(AttributeType.SkillGauge,Mathf.Clamp(GetValue(AttributeType.SkillGauge), 0f, GetValue(AttributeType.MaxSkillGauge)));
            }
        }

        public async UniTask<PlayerStat> GetDataStruct()
        {
            PlayerStat playerDataStat = await GameManager.Instance.GetPlayerStat();
            return PlayerStat.FromDictionary(playerDataStat, _attributeDictionary);
        }
    }
}
