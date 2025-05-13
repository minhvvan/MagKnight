// Assets/Scripts/UI/UIButtonSFX.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]          // Button·Toggle 모두 대응
public class UIButtonSFX : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler
{
    [SerializeField] private string hoverKey = AudioBase.SFX.UI.Menu.Move;
    [SerializeField] private string clickKey = AudioBase.SFX.UI.Menu.Select;

    public void OnPointerEnter(PointerEventData _) =>
        AudioManager.Instance.PlaySFX(hoverKey);     // 2D 재생

    public void OnPointerClick(PointerEventData _) =>
        AudioManager.Instance.PlaySFX(clickKey);
}