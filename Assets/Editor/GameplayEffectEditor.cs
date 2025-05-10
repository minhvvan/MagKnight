#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GameplayEffect))]
public class GameplayEffectEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var effectType = (EffectType)property.FindPropertyRelative("effectType").enumValueIndex;

        int lineCount = effectType switch
        {
            EffectType.Instant => 3,
            EffectType.Duration => 8,
            EffectType.Infinite => 3
        };

        return EditorGUIUtility.singleLineHeight * lineCount + 5;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float y = position.y;

        var effectTypeProp = property.FindPropertyRelative("effectType");
        var attributeTypeProp = property.FindPropertyRelative("attributeType");
        var amountProp = property.FindPropertyRelative("amount");
        var durationProp = property.FindPropertyRelative("duration");
        var periodProp = property.FindPropertyRelative("period");
        var trackingProp = property.FindPropertyRelative("tracking");
        var maxStackProp = property.FindPropertyRelative("maxStack");

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), effectTypeProp);
        y += lineHeight;
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), attributeTypeProp);
        y += lineHeight;
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), amountProp);
        y += lineHeight;

        if(attributeTypeProp.enumValueIndex == (int)AttributeType.Damage)
        {
            var damageTypeProp = property.FindPropertyRelative("damageType");
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), damageTypeProp);
            y += lineHeight;
        }

        var effectType = (EffectType)effectTypeProp.enumValueIndex;

        if (effectType == EffectType.Duration)
        {
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), durationProp);
            y += lineHeight;
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), periodProp);
            y += lineHeight;
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), trackingProp);
            y += lineHeight;
            if(trackingProp.boolValue)
            {
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), maxStackProp);
                y += lineHeight;
            }
        }

        EditorGUI.EndProperty();
    }
}
#endif
