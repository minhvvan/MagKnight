using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ArtifactInventoryUIController : MonoBehaviour, IBasePopupUIController
{
    [SerializeField] private ArtifactSlot artifactSlot;
    [SerializeField] private GameObject Left_ArtifactInventory;
    [SerializeField] private GameObject Right_ArtifactInventory;
    [SerializeField] private GameObject SlotPrefab;
    [SerializeField] private GameObject ArtifactUIPrefab;
    [SerializeField] private Button HideButton;
    
    private List<ArtifactSlot> Left_ArtifactSlots = new List<ArtifactSlot>();
    private List<ArtifactSlot> Right_ArtifactSlots = new List<ArtifactSlot>();
    
    private ArtifactInventory inventory;
    private MagneticController magneticController;
    private PlayerDetailUIController playerDetailUIController;
    
    public void ShowUI(ArtifactDataSO artifactDataSO)
    {
        SetSlotColor(magneticController.GetMagneticType());
        
        if (artifactDataSO != null)
        {
            var artifactUI = Instantiate(ArtifactUIPrefab, artifactSlot.transform).GetComponent<ArtifactUI>();
            artifactUI.Initialized(artifactDataSO, transform);
        }

        for (int i = 0; i < inventory.Left_ArtifactGas.Length; i++)
        {
            if (inventory.Left_ArtifactGas[i] != null)
            {
                if (Left_ArtifactSlots[i].GetChild() == null)
                {
                    var artifactUI = Instantiate(ArtifactUIPrefab, Left_ArtifactSlots[i].transform).GetComponent<ArtifactUI>();
                    artifactUI.Initialized(inventory.Left_ArtifactGas[i], transform);
                }
            }
        }
        
        for (int i = 0; i < inventory.Right_ArtifactGas.Length; i++)
        {
            if (inventory.Right_ArtifactGas[i] != null)
            {
                if (Right_ArtifactSlots[i].GetChild() == null)
                {
                    var artifactUI = Instantiate(ArtifactUIPrefab, Right_ArtifactSlots[i].transform).GetComponent<ArtifactUI>();
                    artifactUI.Initialized(inventory.Right_ArtifactGas[i], transform);
                }
            }
        }
        
        ShowUI();
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
    }
    
    public void HideUI()
    {
        UIManager.Instance.DisableCursor();

        var artifact = artifactSlot.GetChild();
        if (artifact != null)
        {
            artifact.GetComponent<ArtifactUI>().DumpArtifact();
        }
        gameObject.SetActive(false);
    }

    public void SetSlotColor(MagneticType magneticType)
    {
        if (magneticType == MagneticType.N)
        {
            foreach (var nArtifactSlot in Left_ArtifactSlots)
            {
                nArtifactSlot.SetBackgroundColor(Color.red);
            }

            foreach (var sArtifactSlot in Right_ArtifactSlots)
            {
                sArtifactSlot.SetBackgroundColor(Color.blue);
            }
        }
        else
        {
            foreach (var nArtifactSlot in Left_ArtifactSlots)
            {
                nArtifactSlot.SetBackgroundColor(Color.blue);
            }

            foreach (var sArtifactSlot in Right_ArtifactSlots)
            {
                sArtifactSlot.SetBackgroundColor(Color.red);
            }
        }
    }
    
    public void Initialized()
    {
        for (int i = 0; i < 9; i++)
        {
            var instance1 = Instantiate(SlotPrefab, Left_ArtifactInventory.transform).GetComponent<ArtifactSlot>();
            instance1.SetBackgroundColor(Color.red);
            instance1.SetSlotIndex(i);
            instance1.OnArtifactModified = UpdateArtifact_Left;
            Left_ArtifactSlots.Add(instance1);
            
            var instance2 = Instantiate(SlotPrefab, Right_ArtifactInventory.transform).GetComponent<ArtifactSlot>();
            instance2.SetBackgroundColor(Color.blue);
            instance2.SetSlotIndex(i);
            instance2.OnArtifactModified = UpdateArtifact_Right;
            Right_ArtifactSlots.Add(instance2);
        }
        
        inventory = FindObjectOfType<ArtifactInventory>();
        magneticController = FindObjectOfType<MagneticController>();
        playerDetailUIController = UIManager.Instance.popupUIController.playerDetailUIController;
        HideButton.onClick.AddListener(HideUI);
    }

    async void UpdateArtifact_Left(int index, ArtifactDataSO artifact)
    {
        await inventory.SetLeftArtifact(index, artifact);
        playerDetailUIController.UpdateUI();
    }
    
    async void UpdateArtifact_Right(int index, ArtifactDataSO artifact)
    {
        await inventory.SetRightArtifact(index, artifact);
        playerDetailUIController.UpdateUI();
    }
}
