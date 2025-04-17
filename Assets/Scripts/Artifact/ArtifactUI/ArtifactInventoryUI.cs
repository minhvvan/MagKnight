using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ArtifactInventoryUI : MonoBehaviour
{
    [SerializeField] private ArtifactSlot artifactSlot;
    [SerializeField] private GameObject Left_ArtifactInventory;
    [SerializeField] private GameObject Right_ArtifactInventory;
    [SerializeField] private GameObject SlotPrefab;
    [SerializeField] private GameObject ArtifactUIPrefab;
    
    private List<ArtifactSlot> Left_ArtifactSlots = new List<ArtifactSlot>();
    private List<ArtifactSlot> Right_ArtifactSlots = new List<ArtifactSlot>();

    private ArtifactSlot beginArtifactSlot;
    private ArtifactInventory inventory;
    private MagneticController magneticController;
    
    void Awake()
    {
        Initialized();
        inventory = FindObjectOfType<ArtifactInventory>();
        magneticController = FindObjectOfType<MagneticController>();
        Hide();
    }

    public void Show(ArtifactDataSO artifactDataSO = null)
    {
        gameObject.SetActive(true);

        SetSlotColor(magneticController.GetMagneticType());
        
        if (artifactDataSO != null)
        {
            var artifactUI = Instantiate(ArtifactUIPrefab, artifactSlot.transform).GetComponent<ArtifactUI>();
            artifactUI.Initialized(artifactDataSO, transform);
        }
    }
    
    public void Hide()
    {
        var artifact = artifactSlot.Icon();
        if (artifact != null)
            Destroy(artifact);
        gameObject.SetActive(false);
    }

    private void SetSlotColor(MagneticType magneticType)
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
    
    void Initialized()
    {
        for (int i = 0; i < 15; i++)
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
    }

    void UpdateArtifact_Left(int index, ArtifactDataSO artifact)
    {
        inventory.SetLeftArtifact(index, artifact);
    }
    
    void UpdateArtifact_Right(int index, ArtifactDataSO artifact)
    {
        inventory.SetRightArtifact(index, artifact);
    }
    
}
