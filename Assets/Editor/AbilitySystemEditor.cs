using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AbilitySystem))]
public class AbilitySystemEditor : Editor
{
    // 인스펙터 UI 상태 변수
    private bool showAttributesInfo = true;
    private bool showActiveEffectsInfo = true;
    private bool showEffectCreator = true;
    
    // 이펙트 생성 필드
    private EffectType newEffectType = EffectType.Instant;
    private AttributeType newAttributeType;
    private float newAmount = 0f;
    private float newDuration = 3f;
    private bool newTracking = true;
    
    // 스타일
    private GUIStyle headerStyle;
    private GUIStyle subHeaderStyle;
    private GUIStyle effectBoxStyle;
    private GUIStyle attributeNameStyle;
    private GUIStyle timeRemainingStyle;
    private Color[] effectTypeColors = new Color[] 
    {
        new Color(0.9f, 0.5f, 0.5f, 0.8f), // Instant - 빨간색
        new Color(0.5f, 0.7f, 0.9f, 0.8f), // Duration - 파란색
        new Color(0.6f, 0.9f, 0.6f, 0.8f)  // Infinite - 초록색
    };

    // 활성 이펙트의 남은 시간 추적 Dictionary
    private Dictionary<int, float> effectRemainingTimes = new Dictionary<int, float>();
    private Dictionary<int, float> effectStartTimes = new Dictionary<int, float>();
    
    // 초기화시 첫 번째 어트리뷰트 타입 선택
    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
        
        // 어트리뷰트 타입 초기화
        var abilitySystem = (AbilitySystem)target;
        var attributeTypes = GetAttributeTypes(abilitySystem);
        if (attributeTypes != null && attributeTypes.Count > 0)
        {
            newAttributeType = attributeTypes[0];
        }
    }

    public override void OnInspectorGUI()
    {
        // 스타일 초기화
        InitStyles();
        
        // 기본 인스펙터를 그리기
        base.OnInspectorGUI();
        
        // 타겟 컴포넌트 참조 가져오기
        AbilitySystem abilitySystem = (AbilitySystem)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("AbilitySystem 디버깅 정보", headerStyle);
        
        // Attribute 정보 표시
        DrawAttributesInfo(abilitySystem);
        
        // 활성화된 이펙트 정보 표시
        DrawActiveEffectsInfo(abilitySystem);
        
        // 이펙트 생성 UI 표시
        DrawEffectCreator(abilitySystem);
    }

    private void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.margin = new RectOffset(0, 0, 10, 5);
        }

        if (subHeaderStyle == null)
        {
            subHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
            subHeaderStyle.fontSize = 12;
            subHeaderStyle.margin = new RectOffset(0, 0, 5, 5);
        }
        
        if (effectBoxStyle == null)
        {
            effectBoxStyle = new GUIStyle(EditorStyles.helpBox);
            effectBoxStyle.padding = new RectOffset(10, 10, 10, 10);
            effectBoxStyle.margin = new RectOffset(0, 0, 5, 5);
        }
        
        if (attributeNameStyle == null)
        {
            attributeNameStyle = new GUIStyle(EditorStyles.boldLabel);
            attributeNameStyle.fontSize = 11;
        }
        
        if (timeRemainingStyle == null)
        {
            timeRemainingStyle = new GUIStyle(EditorStyles.miniLabel);
            timeRemainingStyle.alignment = TextAnchor.MiddleRight;
        }
    }

    private void DrawAttributesInfo(AbilitySystem abilitySystem)
    {
        showAttributesInfo = EditorGUILayout.Foldout(showAttributesInfo, "Attributes", true, EditorStyles.foldoutHeader);
        
        if (showAttributesInfo)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // AttributeSet에서 어트리뷰트 타입 목록 가져오기
            List<AttributeType> attributeTypes = GetAttributeTypes(abilitySystem);
            
            if (attributeTypes != null && attributeTypes.Count > 0)
            {
                foreach (AttributeType type in attributeTypes)
                {
                    try
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        // 현재 값 표시
                        float value = abilitySystem.GetValue(type);
                        EditorGUILayout.LabelField(type.ToString(), GUILayout.Width(120));
                        
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.FloatField(value, GUILayout.Width(80));
                        EditorGUI.EndDisabledGroup();
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    catch (Exception ex)
                    {
                        EditorGUILayout.LabelField($"Error displaying {type}: {ex.Message}", EditorStyles.miniLabel);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("AttributeSet이 초기화되지 않았거나 어트리뷰트가 없습니다.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
    }

    // AttributeSet에서 어트리뷰트 타입 목록 가져오기
    private List<AttributeType> GetAttributeTypes(AbilitySystem abilitySystem)
    {
        try
        {
            var attributeSet = GetAttributeSet(abilitySystem);
            if (attributeSet != null)
            {
                // AttributeSet.GetAttributeTypes() 메서드 호출
                return attributeSet.GetAttributeTypes().ToList();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"GetAttributeTypes 오류: {ex.Message}");
        }
        
        return new List<AttributeType>();
    }
    
    // AbilitySystem에서 AttributeSet 가져오기
    private AttributeSet GetAttributeSet(AbilitySystem abilitySystem)
    {
        var attributesField = typeof(AbilitySystem).GetField("Attributes", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (attributesField != null)
        {
            return attributesField.GetValue(abilitySystem) as AttributeSet;
        }
        return null;
    }

    private void ApplyTestEffect(AbilitySystem abilitySystem, AttributeType type, float amount)
    {
        try
        {
            GameplayEffect testEffect = new GameplayEffect(EffectType.Instant, type, amount);
            abilitySystem.ApplyEffect(testEffect);
        }
        catch (Exception ex)
        {
            Debug.LogError($"테스트 이펙트 적용 오류: {ex.Message}");
        }
    }

    private void DrawActiveEffectsInfo(AbilitySystem abilitySystem)
    {
        showActiveEffectsInfo = EditorGUILayout.Foldout(showActiveEffectsInfo, "활성 이펙트", true, EditorStyles.foldoutHeader);
        
        if (showActiveEffectsInfo)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // _activatedEffects Dictionary 가져오기 (리플렉션을 통해)
            var activatedEffects = GetActivatedEffects(abilitySystem);
            
            if (activatedEffects != null && activatedEffects.Count > 0)
            {
                // 에러 로그 방지를 위해 try-catch 사용
                try
                {
                    foreach (var effect in activatedEffects)
                    {
                        DrawEffectInfo(effect.Key, effect.Value, abilitySystem);
                    }
                }
                catch (Exception ex)
                {
                    EditorGUILayout.HelpBox($"이펙트 표시 중 오류 발생: {ex.Message}", MessageType.Error);
                }
            }
            else
            {
                EditorGUILayout.LabelField("활성화된 이펙트가 없습니다.");
            }
            
            EditorGUILayout.EndVertical();
        }
    }

    private SerializedDictionary<int, GameplayEffect> GetActivatedEffects(AbilitySystem abilitySystem)
    {
        try 
        {
            FieldInfo fieldInfo = typeof(AbilitySystem).GetField("_activatedEffects", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(abilitySystem) as SerializedDictionary<int, GameplayEffect>;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"활성 이펙트 가져오기 오류: {ex.Message}");
        }
        return null;
    }

    private void DrawEffectInfo(int key, GameplayEffect effect, AbilitySystem abilitySystem)
    {
        GUIStyle boxStyle = new GUIStyle(effectBoxStyle);
        boxStyle.normal.background = MakeColoredTexture(2, 2, effectTypeColors[(int)effect.effectType]);
        
        EditorGUILayout.BeginVertical(boxStyle);
        
        // 이펙트 정보 표시
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(effect.attributeType.ToString(), attributeNameStyle, GUILayout.Width(100));
        EditorGUILayout.LabelField($"효과: {effect.amount:F1}", GUILayout.Width(80));
        
        string typeText = "";
        switch (effect.effectType)
        {
            case EffectType.Instant:
                typeText = "즉시";
                break;
            case EffectType.Duration:
                typeText = "지속";
                break;
            case EffectType.Infinite:
                typeText = "무한";
                break;
        }
        
        EditorGUILayout.LabelField($"타입: {typeText}", GUILayout.Width(80));
        
        // Duration 이펙트에 대한 남은 시간 표시
        if (effect.effectType == EffectType.Duration)
        {
            // 해당 이펙트의 시작 시간 기록
            if (!effectStartTimes.ContainsKey(key))
            {
                effectStartTimes[key] = Time.realtimeSinceStartup;
            }
            
            // 경과된 시간 계산 및 남은 시간 표시
            float elapsedTime = Time.realtimeSinceStartup - effectStartTimes[key];
            float remainingTime = Mathf.Max(0, effect.duration - elapsedTime);
            effectRemainingTimes[key] = remainingTime;
            
            EditorGUILayout.LabelField($"남은 시간: {remainingTime:F1}초", timeRemainingStyle);
        }
        else if (effect.effectType == EffectType.Infinite)
        {
            EditorGUILayout.LabelField("∞", timeRemainingStyle);
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 이펙트 제거 버튼
        if (GUILayout.Button("이펙트 제거"))
        {
            abilitySystem.RemoveEffect(effect);
            effectStartTimes.Remove(key);
            effectRemainingTimes.Remove(key);
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawEffectCreator(AbilitySystem abilitySystem)
    {
        showEffectCreator = EditorGUILayout.Foldout(showEffectCreator, "이펙트 생성", true, EditorStyles.foldoutHeader);
        
        if (showEffectCreator)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 이펙트 타입 선택
            newEffectType = (EffectType)EditorGUILayout.EnumPopup("이펙트 타입", newEffectType);
            
            // 어트리뷰트 타입 선택 - 실제 존재하는 어트리뷰트만 표시
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("어트리뷰트 타입", GUILayout.Width(140));
            
            // 어트리뷰트 타입 목록 가져오기
            var attributeTypes = GetAttributeTypes(abilitySystem);
            
            if (attributeTypes != null && attributeTypes.Count > 0)
            {
                // 선택된 인덱스 찾기
                int selectedIndex = attributeTypes.IndexOf(newAttributeType);
                if (selectedIndex < 0) selectedIndex = 0;
                
                // 드롭다운으로 선택
                selectedIndex = EditorGUILayout.Popup(selectedIndex, attributeTypes.Select(t => t.ToString()).ToArray());
                if (selectedIndex >= 0 && selectedIndex < attributeTypes.Count)
                {
                    newAttributeType = attributeTypes[selectedIndex];
                }
            }
            else
            {
                EditorGUILayout.LabelField("사용 가능한 어트리뷰트가 없습니다");
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 수치 입력
            newAmount = EditorGUILayout.FloatField("수치", newAmount);
            
            // Duration이 아닌 경우 duration 필드 비활성화
            EditorGUI.BeginDisabledGroup(newEffectType != EffectType.Duration);
            newDuration = EditorGUILayout.FloatField("지속 시간", newDuration);
            EditorGUI.EndDisabledGroup();
            
            // Tracking 여부
            newTracking = EditorGUILayout.Toggle("이펙트 추적", newTracking);
            
            // 이펙트 적용 버튼
            if (GUILayout.Button("이펙트 적용", GUILayout.Height(30)))
            {
                try
                {
                    GameplayEffect newEffect = new GameplayEffect(
                        newEffectType,
                        newAttributeType,
                        newAmount,
                        newDuration,
                        newTracking
                    );
                    
                    abilitySystem.ApplyEffect(newEffect);
                    
                    // 생성 후 값 리셋
                    newAmount = 0f;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"이펙트 적용 중 오류 발생: {ex.Message}");
                    EditorUtility.DisplayDialog("이펙트 적용 오류", $"이펙트를 적용하는 중 오류가 발생했습니다: {ex.Message}", "확인");
                }
            }
            
            EditorGUILayout.EndVertical();
        }
    }

    private Texture2D MakeColoredTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (target != null)
        {
            Repaint();
        }
    }
}