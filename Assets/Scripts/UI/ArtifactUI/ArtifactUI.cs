using System.Collections;
using System.Collections.Generic;
using hvvan;
using Moon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 아티팩트 정보를 담고 있는 UI, 드래그 앤 드롭 기능만 가지고 있음, 파괴 시 아티팩트 GameObject로 반환
public class ArtifactUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ArtifactDataSO artifact;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject artifactPrefab;
    
    Vector3 startPos;
    [HideInInspector] public Transform startParent;
    
    [SerializeField] public Transform onDragParent;
    
    [SerializeField] public RectTransform rect;
    public void Initialized(ArtifactDataSO artifact, Transform onDragParent)
    {
        this.artifact = artifact;
        icon.sprite = artifact.icon;
        this.onDragParent = onDragParent;
    }

    public ArtifactDataSO GetArtifact()
    {
        return artifact;
    }
    
    public void SetArtifactIcon(Transform parent)
    {
        transform.SetParent(parent);
        transform.position = parent.position;
    }
    

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = transform.position;
        startParent = transform.parent;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        transform.SetParent(onDragParent);
        AudioManager.Instance.PlaySFX(AudioBase.SFX.UI.Artifact.PickUp);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySFX(AudioBase.SFX.UI.Artifact.Drop);
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // TODO: 아이템 버리기
            transform.SetParent(null);
            startParent.GetComponent<ArtifactSlot>().ModifyArtifact();
            DumpArtifact();
            return;
        }
        
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (transform.parent == onDragParent)
        {
            transform.position = startPos;
            transform.SetParent(startParent);
        }
    }

    public void DumpArtifact()
    {
        var player = FindObjectOfType<PlayerController>().gameObject;
        if(player == null) return;
        
        var instanceArtifact = Instantiate(artifactPrefab).GetComponent<ArtifactObject>();
        instanceArtifact.SetArtifactData(artifact);
        instanceArtifact.transform.position = player.transform.position + player.transform.forward.normalized;
        ItemManager.Instance.AddArtifact(artifact);
        var currentRunData = GameManager.Instance.CurrentRunData;
        currentRunData.artifactsId.Remove(artifact.itemID);
        _ = GameManager.Instance.SaveData(Constants.CurrentRun);
        Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.popupUIController.productUIController.ShowArtifactUI(artifact, rect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.popupUIController.productUIController.HideUI();
    }
}
