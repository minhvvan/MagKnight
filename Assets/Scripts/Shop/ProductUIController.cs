using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ProductUIController : MonoBehaviour, IBasePopupUIController
{
    [Header("Item Profile")]
    public Image   itemImage;
    
    [Header("UI Texts")]
    //public TMP_Text itemCategory;
    //public TMP_Text itemRarity;
    public TMP_Text itemNameAndUpgrade;
    public TMP_Text itemSpec;
    public TMP_Text itemEffect;
    public TMP_Text itemEffectSub;
    //public TMP_Text itemDescription;
    public TMP_Text itemPrice;
    public TMP_Text inputInteract;
    public TMP_Text inputCancel;
    
    [Header("UI HeadPanels")]
    public RectTransform contentRectTr;
    public RectTransform itemFrameRectTr;
    public float itemFrameHeightOrigin;
    public float contentHeightOrigin;
    public float itemFrameHeightOffset;
    public float contentHeightOffset;
    
    [Header("UI ElementPanels")]
    public RectTransform itemEffectRectTr;
    public RectTransform itemEffectSubRectTr;
    public RectTransform inputGuidePanel;
    public RectTransform inputCancelPanel;
    public float guidePanelWidthOrigin;
    public float guidePanelWidthOffset;

    [Header("UI Frame Inner Icon")]
    public Image itemCategoryIcon;
    public Sprite[] itemCategoryIcons;
    
    [Header("UI Frame")]
    public Image itemFrameImage;
    public Image backMenuImage;
    public Image itemEffectBg;
    public Image itemEffectBgSub;
    public Image itemSpecBg;
    public Image inputInteractBg;
    public Image inputCancelBg;
    
    
    [Header("UI Colors")]
    public byte backColorAlpha;
    public Color32[] textColors;
    public Color32[] backColors;
    public Color32[] panelColors;
    
    private GameObject _uiItem;
    private Vector2 _startPosition;
    private RectTransform _rectTransform;
    private CanvasScaler  _canvasScaler;
    
    private bool isHide = false;
    
    public void Initialized()
    {
        _rectTransform = GetComponent<RectTransform>();
        _startPosition = _rectTransform.anchoredPosition;
        _canvasScaler = GetComponentInParent<CanvasScaler>();
        
        contentHeightOrigin = contentRectTr.rect.height;
        itemFrameHeightOrigin = itemFrameRectTr.rect.height;
        guidePanelWidthOrigin = inputGuidePanel.rect.width;
        
        itemSpecBg.color = panelColors[4];
        inputInteractBg.color = panelColors[3];
        inputCancelBg.color = panelColors[4];
    }
    
    public void ShowUI()
    {
        isHide = false;
        _rectTransform.DOKill();
        
        _rectTransform.DOScale(1, 0.1f);
        gameObject.SetActive(true);
    }

    public void HideUI()
    {
        if(isHide) return;
        _rectTransform.DOKill();
        
        _rectTransform.DOScale(0, 0.1f).OnComplete(() =>
        {
            _rectTransform.anchoredPosition = _startPosition;
            gameObject.SetActive(false); 
            inputGuidePanel.gameObject.SetActive(true);
        });
        isHide = true;
    }

    //팝업 레이아웃 기본값으로 초기화.
    private void SetRelocationUI(bool isProduct = false)
    {
        if (isProduct)
        {
            inputInteract.text = "구매(E)";
            inputCancel.text = "";
            inputCancelPanel.gameObject.SetActive(false);
            inputGuidePanel.sizeDelta = new Vector2(guidePanelWidthOrigin - guidePanelWidthOffset, inputGuidePanel.rect.height);
        }
        else
        {
            inputInteract.text = "습득(E)";
            inputCancel.text = "분해(G)";
            inputCancelPanel.gameObject.SetActive(true);
            inputGuidePanel.sizeDelta = new Vector2(guidePanelWidthOrigin, inputGuidePanel.rect.height);
        }
        
        itemEffectSubRectTr.gameObject.SetActive(true);
        inputGuidePanel.gameObject.SetActive(true);
        
        //프레임 크기 초기화.
        itemFrameRectTr.sizeDelta = new Vector2(
            itemFrameRectTr.rect.width, 
            itemFrameHeightOrigin);
        contentRectTr.sizeDelta = new Vector2(
            contentRectTr.rect.width,
            contentHeightOrigin);
        
        //Effect 패널 배경색 초기화
        itemEffectBg.color = panelColors[0];
        itemEffectBgSub.color = panelColors[0];
    }
    
    //아이템 팝업 정보 세팅
    public void SetItemText(GameObject item, bool isProduct = false)
    {
        if (_uiItem != item) _uiItem = item;
        else return;

        SetRelocationUI(isProduct);
        
        if (_uiItem.TryGetComponent(out ArtifactObject artifactObject))
        {
            //아티팩트일 경우 극성 표시로 보이게 변경
            itemEffectBg.color = panelColors[1];
            itemEffectBgSub.color = panelColors[2];
            
            itemImage.sprite = artifactObject.icon;
            itemNameAndUpgrade.text = $"{artifactObject.itemName}";
            
            ConvertSort(artifactObject.category);
            ConvertSort(artifactObject.rarity);
            itemSpec.text += artifactObject.itemDescription;
            
            (itemEffect.text, itemEffectSub.text) = ConvertArtifactEffectToText(artifactObject.GetArtifactData());
            itemPrice.text = artifactObject.scrapValue.ToString();
        }
        else if (_uiItem.TryGetComponent(out MagCore magCore))
        {
            itemImage.sprite = magCore.icon;
            itemNameAndUpgrade.text = $"+{magCore.currentUpgradeValue} {magCore.itemName}";
            
            ConvertSort(magCore.category);
            ConvertSort(magCore.rarity);
            itemSpec.text += magCore.itemDescription;
            
            (itemEffect.text, itemEffectSub.text) = ConvertMagCoreEffectToText(magCore.GetMagCoreSO(), magCore.currentUpgradeValue);
            itemPrice.text = magCore.scrapValue.ToString();
        }
        else if (_uiItem.TryGetComponent(out HealthPack healthPack))
        {
            itemImage.sprite = healthPack.icon;
            itemNameAndUpgrade.text = $"{healthPack.itemName}";
            
            ConvertSort(healthPack.category);
            ConvertSort(healthPack.rarity);
            itemSpec.text += healthPack.itemDescription;
            
            itemEffect.text = $"체력을 {healthPack.healValue}회복합니다.";
            itemPrice.text = healthPack.scrapValue.ToString();
            
            //힐팩 출력시 필요없는 영역 비활성화
            itemEffectSubRectTr.gameObject.SetActive(false);
            itemFrameRectTr.sizeDelta = new Vector2(
                itemFrameRectTr.rect.width, 
                itemFrameHeightOrigin - itemFrameHeightOffset);
            contentRectTr.sizeDelta = new Vector2(
                contentRectTr.rect.width,
                contentHeightOrigin - contentHeightOffset);
        }
        else
        {
            Debug.Log("아이템에 접근할 수 없습니다.");
            _uiItem = null;
        }
    }

    public void ShowArtifactUI(ArtifactDataSO artifactData, RectTransform artifactRect)
    {
        CalculateUIPosition(artifactRect);

        SetRelocationUI();

        //Effect 패널 배경색 초기화
        itemEffectBg.color = panelColors[0];
        itemEffectBgSub.color = panelColors[0];
        
        //아티팩트일 경우 극성 표시로 보이게 변경
        itemEffectBg.color = panelColors[1];
        itemEffectBgSub.color = panelColors[2];
        
        inputGuidePanel.gameObject.SetActive(false);
        
        itemImage.sprite = artifactData.icon;
        itemNameAndUpgrade.text = $"{artifactData.itemName}";
        
        ConvertSort(ItemCategory.Artifact);
        ConvertSort(artifactData.rarity);
        itemSpec.text += artifactData.description;
        
        (itemEffect.text, itemEffectSub.text) = ConvertArtifactEffectToText(artifactData);
        itemPrice.text = artifactData.scrapValue.ToString();
        ShowUI();
    }

    private void CalculateUIPosition(RectTransform artifactRect)
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
        
        _rectTransform.position = artifactRect.position + new Vector3(slotWidth + width, -slotHeight/2);
        Vector2 pos = _rectTransform.position;
        bool rightTruncated = pos.x > Screen.width;
        
        if(rightTruncated)
            _rectTransform.position = artifactRect.position + new Vector3(-slotWidth, -slotHeight/2);
    }

    public void ShowMagCoreUI(MagCore magCore, RectTransform magCoreRect)
    {
        CalculateUIPosition(magCoreRect);
        
        SetRelocationUI();
        
        //Effect 패널 배경색 초기화
        itemEffectBg.color = panelColors[0];
        itemEffectBgSub.color = panelColors[0];
        
        itemImage.sprite = magCore.icon;
        itemNameAndUpgrade.text = $"+{magCore.currentUpgradeValue} {magCore.itemName}";
            
        inputGuidePanel.gameObject.SetActive(false);
        
        ConvertSort(magCore.category);
        ConvertSort(magCore.rarity);
        itemSpec.text += magCore.itemDescription;
            
        (itemEffect.text, itemEffectSub.text) = ConvertMagCoreEffectToText(magCore.GetMagCoreSO(), magCore.currentUpgradeValue);
        itemPrice.text = magCore.scrapValue.ToString();
        
        ShowUI();
    }
    
    private void ConvertSort(Enum text)
    {
        //TODO: 텍스트 변환을 프레임 컬러 변환, 아이콘 변환으로 리팩토링 할 것.
        
        if (text.GetType() == typeof(ItemCategory))
        {
            switch (text)
            {
                case ItemCategory.Artifact:
                    itemSpec.text = "[아티팩트]";
                    itemCategoryIcon.sprite = itemCategoryIcons[0];
                    return;
                case ItemCategory.MagCore:
                    itemSpec.text  = "[파츠]";
                    itemCategoryIcon.sprite = itemCategoryIcons[1];
                    return;
                case ItemCategory.HealthPack:
                    itemSpec.text = "[회복]";
                    itemCategoryIcon.sprite = itemCategoryIcons[2];
                    return;
            }
        }
        else if (text.GetType() == typeof(ItemRarity))
        {
            switch (text)
            {
                case ItemRarity.Common:
                    itemSpec.text += "[일반]\n";
                    itemFrameImage.color = textColors[1];
                    backColors[1].a = backColorAlpha;
                    backMenuImage.color = backColors[1];
                    return;
                case ItemRarity.Uncommon:
                    itemSpec.text += "[특별]\n";
                    itemFrameImage.color = textColors[2];
                    backColors[2].a = backColorAlpha;
                    backMenuImage.color = backColors[2];
                    return;
                case ItemRarity.Rare:
                    itemSpec.text += "[희귀]\n";
                    itemFrameImage.color = textColors[3];
                    backColors[3].a = backColorAlpha;
                    backMenuImage.color = backColors[3];
                    return;
                case ItemRarity.Epic:
                    itemSpec.text += "[서사]\n";
                    itemFrameImage.color = textColors[4];
                    backColors[4].a = backColorAlpha;
                    backMenuImage.color = backColors[4];
                    return;
                case ItemRarity.Legendary:
                    itemSpec.text += "[전설]\n";
                    itemFrameImage.color = textColors[5];
                    backColors[5].a = backColorAlpha;
                    backMenuImage.color = backColors[5];
                    return;
            }
        }
        
        Debug.Log("알맞는 변환 타입이 없습니다.");
    }

    private (string, string) ConvertArtifactEffectToText(ArtifactDataSO artifactData)
    {
        //string completeText = "";
        var dataNText = string.Empty;
        var dataSText = string.Empty;
        itemEffectSubRectTr.gameObject.SetActive(true);
        
        //프레임 크기 초기화.
        itemFrameRectTr.sizeDelta = new Vector2(
            itemFrameRectTr.rect.width, 
            itemFrameHeightOrigin);
        contentRectTr.sizeDelta = new Vector2(
            contentRectTr.rect.width,
            contentHeightOrigin);
        
        switch (artifactData)
        {
            case PassiveArtifactDataSO passiveArtifactData:
                
                var passiveDataN = passiveArtifactData.N_passiveArtifacts;
                foreach (var data in passiveDataN)
                {
                    dataNText += ParsePassiveEffectData(data);
                }
                var passiveDataS = passiveArtifactData.S_passiveArtifacts;
                foreach (var data in passiveDataS)
                {
                    dataSText += ParsePassiveEffectData(data);
                }

                if (dataNText == dataSText)
                {
                    dataNText = $"[단극 효과]\n{dataNText}";
                    dataSText = "";
                    itemEffectSubRectTr.gameObject.SetActive(false);
                    itemFrameRectTr.sizeDelta = new Vector2(
                        itemFrameRectTr.rect.width, 
                        itemFrameHeightOrigin - itemFrameHeightOffset);
                    contentRectTr.sizeDelta = new Vector2(
                        contentRectTr.rect.width,
                        contentHeightOrigin - contentHeightOffset);
                }
                else
                {
                    dataNText = $"[N극 효과]\n{dataNText}";
                    dataSText = $"[S극 효과]\n{dataSText}";
                }
                
                return (dataNText, dataSText);
            case StatArtifactDataSO statArtifactData:
                
                var statDataN = statArtifactData.N_ArtifactEffect;
                foreach (var data in statDataN)
                {
                    dataNText += ParseGameplayEffect(data);
                }
                var statDataS = statArtifactData.S_ArtifactEffect;
                foreach (var data in statDataS)
                {
                    dataSText += ParseGameplayEffect(data);
                }
                
                if (dataNText == dataSText)
                {
                    dataNText = $"[단극 효과]\n{dataNText}";
                    dataSText = "";
                    itemEffectSubRectTr.gameObject.SetActive(false);
                    itemFrameRectTr.sizeDelta = new Vector2(
                        itemFrameRectTr.rect.width, 
                        itemFrameHeightOrigin - itemFrameHeightOffset);
                    contentRectTr.sizeDelta = new Vector2(
                        contentRectTr.rect.width,
                        contentHeightOrigin - contentHeightOffset);
                }
                else
                {
                    dataNText = $"[N극 효과]\n{dataNText}";
                    dataSText = $"[S극 효과]\n{dataSText}";
                }
                
                return (dataNText, dataSText);
        }

        return (null,null);
    }
    
    private (string, string) ConvertMagCoreEffectToText(MagCoreSO magCoreData, int upgradeValue)
    {
        var completeText = string.Empty;
        var partsEffectText = string.Empty;
        var magnetEffectText = string.Empty;
        //itemSpec.text = "개별 능력치 없음.";
        
        partsEffectText += "[파츠 고유 효과]\n";
        var magCoreGameEffects = magCoreData.gameplayEffects;
        foreach (var keyPair in magCoreGameEffects)
        {
            if (keyPair.Key == upgradeValue)
            {
                foreach (var data in keyPair.Value)
                {
                    if (data == null) break;
                    partsEffectText +=$"{ParseGameplayEffect(data)}\n";
                }
            }
        }
        var magCorePassiveEffects = magCoreData.passiveEffects;
        foreach (var keyPair in magCorePassiveEffects)
        {
            if (keyPair.Key == upgradeValue)
            {
                foreach (var data in keyPair.Value)
                {
                    if (data == null) break;
                    partsEffectText += ParsePassiveEffectData(data);
                }
            }
        }
        
        
        magnetEffectText += "[극성 전환 효과]\n";
        var magnetGameEffects = magCoreData.magnetGameplayEffects;
        foreach (var keyPair in magnetGameEffects)
        {
            if (keyPair.Key == upgradeValue)
            {
                foreach (var data in keyPair.Value)
                {
                    if (data == null) break;
                    magnetEffectText += ParseGameplayEffect(data);
                }
            }
        }
        var magnetPassiveEffects = magCoreData.magnetPassiveEffects;
        foreach (var keyPair in magnetPassiveEffects)
        {
            if (keyPair.Key == upgradeValue)
            {
                foreach (var data in keyPair.Value)
                {
                    if (data == null) break;
                    magnetEffectText += ParsePassiveEffectData(data);
                }
            }
        }
       
        
        return (partsEffectText, magnetEffectText);
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
                completeText += $"죽음에 이르는 피해를 받으면 ";
                break;
            case TriggerEventType.OnMagnetic:
                completeText += $"자석능력 사용 시 ";
                break;
            case TriggerEventType.OnSkill:
                completeText += $"스킬 사용 시 ";
                break;
        }
        
        if (chance < 1f)
        {
            completeText += $"{chance*100f}%의 확률로 ";
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

        if (isTarget) completeText += "대상에게 ";
        else if (!isTarget) completeText += "라이언의 ";

        bool isPercent = false;
        
        completeText += AttributeTypeToText(attributeType).Item1;
        isPercent = AttributeTypeToText(attributeType).Item2;

        if (effectType == EffectType.Duration)
        {
            if (period > 0) completeText += $"{period}초 마다 ";
        }

        string addText = string.Empty;
        // if (!isTarget)
        // {
        //     if (amount > 0) addText += "증가";
        //     else if (amount < 0) addText += "감소";
        // }
        // else if (isTarget)
        // {
        //     if (amount > 0) addText += "감소";
        //     else if (amount < 0) addText += "증가";
        // }

        if (attributeType == AttributeType.Damage && amount > 0)
        {
            completeText += $"{amount}가합니다.";
        }
        else
        {
            if (amount > 0) addText += "증가";
            else if (amount < 0) addText += "감소";

            if (attributeType == AttributeType.Defense)
            { 
                if (amount < 0) amount *= -1f;
            }

            if(!isPercent) completeText += $"{amount} {addText}시킵니다.";
            else if(isPercent) completeText += $"{amount*100f}% {addText}시킵니다.";
        }
        
        

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

        //completeText += "라이언의 ";

        bool isPercent = false;

        completeText += AttributeTypeToText(attributeType).Item1;
        isPercent = AttributeTypeToText(attributeType).Item2;
        
        //끝에서 두글자 제거.
        completeText = completeText.Substring(0, completeText.Length - 2);

        if (effectType == EffectType.Duration)
        {
            if (period > 0) completeText += $"{period}초 마다 ";
        }

        string addText = string.Empty;
        if (effectType != EffectType.Duration)
        {
            if (amount > 0) addText += "+";
            else if (amount < 0) addText += "-";

            if(!isPercent) completeText += $" {addText}{amount}";
            else if(isPercent) completeText += $" {addText}{amount*100f}%";
        }
        else
        {
            if (amount > 0) addText += "증가";
            else if (amount < 0) addText += "감소";
        
            if(!isPercent) completeText += $"{amount} {addText}시킵니다.";
            else if(isPercent) completeText += $"{amount*100f}% {addText}시킵니다.";
        }

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
                completeText += $"피해를 ";
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
