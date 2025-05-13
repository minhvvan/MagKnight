using hvvan;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MagCoreUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image icon;
    [SerializeField] RectTransform rect;
    
    private MagCore magCore;
    
    public void SetIcon()
    {
        magCore = GameManager.Instance.Player.WeaponHandler.currentMagCore; 
        icon.sprite = magCore.icon;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.popupUIController.productUIController.ShowMagCoreUI(magCore, rect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.popupUIController.productUIController.HideUI();
    }
}
