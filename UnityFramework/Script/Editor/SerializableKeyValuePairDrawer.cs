using UnityEditor;
using UnityEngine;

/// <summary>
/// SerializableKeyValuePair용 PropertyDrawer
/// Inspector에서 키-밸류를 가로로 표시
/// </summary>
[CustomPropertyDrawer(typeof(SerializableKeyValuePair<,>))]
public class SerializableKeyValuePairDrawer : PropertyDrawer
{
    private const float KEY_LABEL_WIDTH = 30f;
    private const float VALUE_LABEL_WIDTH = 45f;
    private const float SPACING = 5f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 키, 밸류 프로퍼티 가져오기
        var keyProperty = property.FindPropertyRelative("key");
        var valueProperty = property.FindPropertyRelative("value");

        // 전체 영역에서 라벨 제외한 너비 계산
        float totalWidth = position.width;
        float availableWidth = totalWidth - KEY_LABEL_WIDTH - VALUE_LABEL_WIDTH - SPACING * 3;
        float fieldWidth = availableWidth / 2;

        float x = position.x;
        float y = position.y;
        float height = EditorGUIUtility.singleLineHeight;

        // Key 라벨
        var keyLabelRect = new Rect(x, y, KEY_LABEL_WIDTH, height);
        EditorGUI.LabelField(keyLabelRect, "Key");
        x += KEY_LABEL_WIDTH + SPACING;

        // Key 필드
        var keyRect = new Rect(x, y, fieldWidth, height);
        EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
        x += fieldWidth + SPACING;

        // Value 라벨
        var valueLabelRect = new Rect(x, y, VALUE_LABEL_WIDTH, height);
        EditorGUI.LabelField(valueLabelRect, "Value");
        x += VALUE_LABEL_WIDTH + SPACING;

        // Value 필드
        var valueRect = new Rect(x, y, fieldWidth, height);
        EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
