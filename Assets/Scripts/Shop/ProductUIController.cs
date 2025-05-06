using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProductUIController : MonoBehaviour, IBasePopupUIController
{
    public Image   itemImage;
    public TMP_Text itemCategory;
    public TMP_Text itemRarity;
    public TMP_Text itemNameAndUpgrade;
    public TMP_Text itemEffect;
    public TMP_Text itemDescription;
    public TMP_Text itemPrice;
    public TMP_Text inputInteract;
    public TMP_Text inputCancel;
    
    public RectTransform contentRectTr;
    public RectTransform itemEffectRectTr;
    public GameObject inputGuidePanel;
   
    public Color32[] textColors;

    private GameObject _uiItem;
    private Vector2 _startPosition;
    private RectTransform _rectTransform;
    private CanvasScaler _canvasScaler;
    
    public void Initialized()
    {
        _rectTransform = GetComponent<RectTransform>();
        _startPosition = _rectTransform.anchoredPosition;
        _canvasScaler = GetComponentInParent<CanvasScaler>();
    }
    
    public void ShowUI()
    {
        gameObject.SetActive(true);
    }

    public void HideUI()
    {
        _rectTransform.anchoredPosition = _startPosition;
        inputGuidePanel.SetActive(true);
        gameObject.SetActive(false);
    }
    
    public void SetItemText(GameObject item, bool isProduct = false)
    {
        if (_uiItem != item) _uiItem = item;
        else return;

        if (isProduct)
        {
            inputInteract.text = "구매";
            inputCancel.text = "";
        }
        else
        {
            inputInteract.text = "습득";
            inputCancel.text = "분해";
        }
        
        if (_uiItem.TryGetComponent(out ArtifactObject artifactObject))
        {
            itemImage.sprite = artifactObject.icon;
            ConvertSortText(artifactObject.category);
            ConvertSortText(artifactObject.rarity);
            itemNameAndUpgrade.text = $"{artifactObject.itemName}";
            itemEffect.text = ConvertArtifactEffectToText(artifactObject.GetArtifactData());
            itemDescription.text = artifactObject.itemDescription;
            itemPrice.text = artifactObject.scrapValue.ToString();
        }
        else if (_uiItem.TryGetComponent(out MagCore magCore))
        {
            itemImage.sprite = magCore.icon;
            ConvertSortText(magCore.category);
            ConvertSortText(magCore.rarity);
            itemNameAndUpgrade.text = $"+{magCore.currentUpgradeValue} {magCore.itemName}";
            itemEffect.text = ConvertMagCoreEffectToText(magCore.GetMagCoreSO(), magCore.currentUpgradeValue);
            itemDescription.text = magCore.itemDescription;
            itemPrice.text = magCore.scrapValue.ToString();
        }
        else if (_uiItem.TryGetComponent(out HealthPack healthPack))
        {
            itemImage.sprite = healthPack.icon;
            ConvertSortText(healthPack.category);
            ConvertSortText(healthPack.rarity);
            itemNameAndUpgrade.text = $"{healthPack.itemName}";
            itemEffect.text = $"체력을 {healthPack.healValue}회복합니다.";
            itemDescription.text = healthPack.itemDescription;
            itemPrice.text = healthPack.scrapValue.ToString();
        }
        else
        {
            Debug.Log("아이템에 접근할 수 없습니다.");
            _uiItem = null;
        }
    }

    public void ShowArtifactUI(ArtifactDataSO artifactData, RectTransform artifactRect)
    {
        float wRatio = Screen.width / _canvasScaler.referenceResolution.x;
        float hRatio = Screen.height / _canvasScaler.referenceResolution.y;
        float ratio =
            wRatio * (1f - _canvasScaler.matchWidthOrHeight) +
            hRatio * (_canvasScaler.matchWidthOrHeight);

        float slotWidth = artifactRect.rect.width * ratio;
        float slotHeight = artifactRect.rect.height * ratio;
        // 툴팁의 크기
        float width = _rectTransform.rect.width * ratio;
        float height = _rectTransform.rect.height * ratio;
        
        _rectTransform.position = artifactRect.position + new Vector3(slotWidth + width / 2, -slotHeight);
        Vector2 pos = _rectTransform.position;
        bool rightTruncated = pos.x + width > Screen.width;
        
        if(rightTruncated)
            _rectTransform.position = artifactRect.position + new Vector3(-slotWidth - width / 2, -slotHeight);
        
        inputGuidePanel.SetActive(false);
        itemImage.sprite = artifactData.icon;
        ConvertSortText(ItemCategory.Artifact);
        ConvertSortText(artifactData.rarity);
        itemNameAndUpgrade.text = $"{artifactData.itemName}";
        itemEffect.text = ConvertArtifactEffectToText(artifactData);
        itemDescription.text = artifactData.description;
        itemPrice.text = artifactData.scrapValue.ToString();
        ShowUI();
    }
    
    private void ConvertSortText(Enum text)
    {
        if (text.GetType() == typeof(ItemCategory))
        {
            switch (text)
            {
                case ItemCategory.Artifact:
                    itemCategory.text = "[아티팩트]";
                    return;
                case ItemCategory.MagCore:
                    itemCategory.text  = "[파츠]";
                    return;
                case ItemCategory.HealthPack:
                    itemCategory.text = "[회복]";
                    return;
            }
        }
        else if (text.GetType() == typeof(ItemRarity))
        {
            switch (text)
            {
                case ItemRarity.Common:
                    itemRarity.text = "[일반]";
                    itemRarity.color = textColors[1];
                    return;
                case ItemRarity.Uncommon:
                    itemRarity.text = "[특별]";
                    itemRarity.color = textColors[2];
                    return;
                case ItemRarity.Rare:
                    itemRarity.text = "[희귀]";
                    itemRarity.color = textColors[3];
                    return;
                case ItemRarity.Epic:
                    itemRarity.text = "[서사]";
                    itemRarity.color = textColors[4];
                    return;
                case ItemRarity.Legendary:
                    itemRarity.text = "[전설]";
                    itemRarity.color = textColors[5];
                    return;
            }
        }
        
        Debug.Log("알맞는 변환 타입이 없습니다.");
    }

    private string ConvertArtifactEffectToText(ArtifactDataSO artifactData)
    {
        string completeText = "";
        switch (artifactData)
        {
            case PassiveArtifactDataSO passiveArtifactData:
                var passiveDataNText = string.Empty;
                var passiveDataSText = string.Empty;
                
                var passiveDataN = passiveArtifactData.N_passiveArtifacts;
                foreach (var data in passiveDataN)
                {
                    passiveDataNText += ParsePassiveEffectData(data);
                }
                var passiveDataS = passiveArtifactData.S_passiveArtifacts;
                foreach (var data in passiveDataS)
                {
                    passiveDataSText += ParsePassiveEffectData(data);
                }

                if (passiveDataNText == passiveDataSText)
                {
                    completeText += "[단극 효과]\n";
                    completeText += passiveDataNText;
                }
                else
                {
                    completeText += "[N극 효과]\n";
                    completeText += passiveDataNText;
                    completeText += "\n[S극 효과]\n";
                    completeText += passiveDataSText;
                }
                
                return completeText;
            case StatArtifactDataSO statArtifactData:
                var statDataNText = string.Empty;
                var statDataSText = string.Empty;
                
                var statDataN = statArtifactData.N_ArtifactEffect;
                foreach (var data in statDataN)
                {
                    statDataNText += ParseGameplayEffect(data);
                }
                var statDataS = statArtifactData.S_ArtifactEffect;
                foreach (var data in statDataS)
                {
                    statDataSText += ParseGameplayEffect(data);
                }
                
                if (statDataNText == statDataSText)
                {
                    completeText += "[단극 효과]\n";
                    completeText += statDataNText;
                }
                else
                {
                    completeText += "[N극 효과]\n";
                    completeText += statDataNText;
                    completeText += "\n[S극 효과]\n";
                    completeText += statDataSText;
                }
                
                return completeText;
        }

        return null;
    }
    
    private string ConvertMagCoreEffectToText(MagCoreSO magCoreData, int upgradeValue)
    {
        var completeText = string.Empty;

        completeText += "[파츠 고유 효과]\n";
        var magCorePassiveEffects = magCoreData.passiveEffects;
        foreach (var keyPair in magCorePassiveEffects)
        {
            if (keyPair.Key == upgradeValue)
            {
                foreach (var data in keyPair.Value)
                {
                    if (data == null) break;
                    completeText += ParsePassiveEffectData(data);
                }
            }
        }
        var magCoreGameEffects = magCoreData.gameplayEffects;
        foreach (var keyPair in magCoreGameEffects)
        {
            if (keyPair.Key == upgradeValue)
            {
                foreach (var data in keyPair.Value)
                {
                    if (data == null) break;
                    completeText += ParseGameplayEffect(data);
                }
            }
        }
        
        completeText += "\n[극성 전환 효과]\n";
        var magnetPassiveEffects = magCoreData.magnetPassiveEffects;
        foreach (var keyPair in magnetPassiveEffects)
        {
            if (keyPair.Key == upgradeValue)
            {
                foreach (var data in keyPair.Value)
                {
                    if (data == null) break;
                    completeText += ParsePassiveEffectData(data);
                }
            }
        }
        var magnetGameEffects = magCoreData.magnetGameplayEffects;
        foreach (var keyPair in magnetGameEffects)
        {
            if (keyPair.Key == upgradeValue)
            {
                foreach (var data in keyPair.Value)
                {
                    if (data == null) break;
                    completeText += ParseGameplayEffect(data);
                }
            }
        }
        
        return completeText;
    }

    private string ParsePassiveEffectData(PassiveEffectData data)
    {
        var completeText = string.Empty;
        var chance = data.triggerChance;
        var eventType = data.triggerEvent;

        var effect = data.effect;
        var effectType = effect.effectType;
        var attributeType = effect.attributeType;
        var amount = effect.amount;
        var duration = effect.duration;
        var period = effect.period;
        var tracking = effect.tracking;
        var maxStack = effect.maxStack;

        var isTarget = data.isTarget;
        var hasCount = data.hasCount;
        var triggerCount = data.triggerCount;

        if (chance < 1f)
        {
            completeText += $"{chance*100f}% 의 확률로 ";
        }

        switch (eventType)
        {
            case TriggerEventType.OnHit:
                completeText += $"타격 시 ";
                break;
            case TriggerEventType.OnDamage:
                completeText += $"피격 시 ";
                break;
            case TriggerEventType.OnAttack:
                completeText += $"공격 시 ";
                break;
            case TriggerEventType.OnDeath:
                completeText += $"사망 시 ";
                break;
            case TriggerEventType.OnMagnetic:
                completeText += $"자석능력 사용 시 ";
                break;
            case TriggerEventType.OnSkill:
                completeText += $"스킬 사용 시 ";
                break;
        }

        // switch (effectType)
        // {
        //     case EffectType.Instant:
        //         completeText += " ";
        //         break;
        //     case EffectType.Duration:
        //         completeText += "일시적으로 ";
        //         break;
        //     case EffectType.Infinite:
        //         completeText += "지속적으로 ";
        //         break;
        // }

        if (effectType == EffectType.Duration)
        {
            if (duration > 0) completeText += $"{duration}초 동안 ";
        }

        if (isTarget) completeText += "대상의 ";
        else if (!isTarget) completeText += "라이언의 ";

        bool isPercent = false;
        
        completeText += AttributeTypeToText(attributeType).Item1;
        isPercent = AttributeTypeToText(attributeType).Item2;

        if (effectType == EffectType.Duration)
        {
            if (period > 0) completeText += $"{period}초 마다 ";
        }

        string addText = string.Empty;
        if (!isTarget)
        {
            if (amount > 0) addText += "증가";
            else if (amount < 0) addText += "감소";
        }
        else if (isTarget)
        {
            if (amount > 0) addText += "감소";
            else if (amount < 0) addText += "증가";
        }

        if(!isPercent) completeText += $"{amount} {addText}시킵니다.";
        else if(isPercent) completeText += $"{amount*100f}% {addText}시킵니다.";

        if (effectType == EffectType.Duration)
        {
            if (maxStack > 0) completeText += $"\n이 효과는 최대 {maxStack}번 중첩됩니다.";
        }

        if (hasCount)
        {
            completeText += $"\n 이 효과는 {triggerCount}번만 사용할 수 있습니다.";
        }
        
        
        return completeText;
    }
    
    private string ParseGameplayEffect(GameplayEffect data)
    {
        var completeText = string.Empty;
        var effectType = data.effectType;
        var attributeType = data.attributeType;
        var amount = data.amount;
        var duration = data.duration;
        var period = data.period;
        var tracking = data.tracking;
        
        // switch (effectType)
        // {
        //     case EffectType.Instant:
        //         completeText += " ";
        //         break;
        //     case EffectType.Duration:
        //         completeText += "일시적으로 ";
        //         break;
        //     case EffectType.Infinite:
        //         completeText += "지속적으로 ";
        //         break;
        // }
        
        if (effectType == EffectType.Duration)
        {
            if (duration > 0) completeText += $"{duration}초 동안 ";
        }

        completeText += "라이언의 ";

        bool isPercent = false;

        completeText += AttributeTypeToText(attributeType).Item1;
        isPercent = AttributeTypeToText(attributeType).Item2;

        if (effectType == EffectType.Duration)
        {
            if (period > 0) completeText += $"{period}초 마다 ";
        }

        string addText = string.Empty;
        if (amount > 0) addText += "증가";
        else if (amount < 0) addText += "감소";

        if(!isPercent) completeText += $"{amount} {addText}시킵니다.";
        else if(isPercent) completeText += $"{amount*100f}% {addText}시킵니다.";

        return completeText;
    }
    
    private (string, bool) AttributeTypeToText(AttributeType attributeType)
    {
        var completeText = string.Empty;
        bool isPercent = false;

        switch (attributeType)
        {
            case AttributeType.MaxHP:
                completeText += $"최대 체력을 ";
                break;
            case AttributeType.HP:
                completeText += $"현재 체력을 ";
                break;
            case AttributeType.Strength:
                completeText += $"공격력을 ";
                break;
            case AttributeType.Defense:
                completeText += $"방어력을 ";
                break;
            case AttributeType.CriticalRate:
                completeText += $"치명타 확률을 ";
                isPercent = true;
                break;
            case AttributeType.CriticalDamage:
                completeText += $"치명타 피해량을 ";
                break;
            case AttributeType.MoveSpeed:
                completeText += $"이동 속도를 ";
                isPercent = true;
                break;
            case AttributeType.AttackSpeed:
                completeText += $"공격 속도를 ";
                isPercent = true;
                break;
            case AttributeType.Damage:
                completeText += $"받는 피해량을 ";
                break;
            case AttributeType.MaxResistance:
                completeText += $"최대 저항력을 ";
                break;
            case AttributeType.Resistance:
                completeText += $"현재 저항력을 ";
                break;
            case AttributeType.ResistanceDamage:
                completeText += $"저항 피해량을 ";
                break;
            case AttributeType.Gold:
                completeText += $"재화를 ";
                break;
            case AttributeType.Impulse:
                completeText += $"충격량을 ";
                break;
            case AttributeType.EndureImpulse:
                completeText += $"강인도를 ";
                break;
            case AttributeType.MaxSkillGauge:
                completeText += $"스킬 최대 충전량을 ";
                break;
            case AttributeType.SkillGauge:
                completeText += $"스킬 충전량을 ";
                break;
            case AttributeType.MagneticRange:
                completeText += $"자석능력 사거리를 ";
                break;
            case AttributeType.MagneticPower:
                completeText += $"자석능력 위력을 ";
                break;
        }

        return (completeText, isPercent);
    }
}
