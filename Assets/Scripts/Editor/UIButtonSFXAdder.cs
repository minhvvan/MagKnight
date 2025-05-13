// Assets/Editor/UIButtonSFXAdder.cs
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///   현재 씬에 존재하는 모든 <see cref="Button"/> 오브젝트에
///   <see cref="UIButtonSFX"/> 컴포넌트를 자동 부착한다.
///   - Undo 지원: 메뉴 실행 직후 Ctrl-Z 로 전부 롤백 가능
///   - 비활성(비어있는) 씬 객체 포함
/// </summary>
public static class UIButtonSFXAdder
{
    private const string MenuPath = "Tools/UI/Add UIButtonSFX to ALL Buttons in Scene";

    [MenuItem(MenuPath)]
    private static void AddSfx()
    {
        int added = 0;
        foreach (Button btn in Object.FindObjectsOfType<Button>(includeInactive: true))
        {
            if (!btn.GetComponent<UIButtonSFX>())
            {
                Undo.AddComponent<UIButtonSFX>(btn.gameObject); // Ctrl-Z 지원
                added++;
            }
        }

        string msg = added == 0
            ? "모든 Button에 이미 UIButtonSFX가 있습니다."
            : $"UIButtonSFX를 {added}개 버튼에 추가했습니다.";
        Debug.Log($"[UIButtonSFXAdder] {msg}");
    }

    // 메뉴 항목 비활성화 조건 ― Scene 에 Button 이 하나도 없으면 비활성
    [MenuItem(MenuPath, /*isValidateFunction=*/ true)]
    private static bool ValidateMenu()
        => Object.FindObjectsOfType<Button>(includeInactive: true).Length > 0;
}