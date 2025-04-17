using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArtifactInventoryUI : MonoBehaviour
{
    [SerializeField] private ArtifactSlot artifactSlot;
    [SerializeField] private GameObject N_ArtifactInventory;
    [SerializeField] private GameObject S_ArtifactInventory;
    [SerializeField] private GameObject SlotPrefab;
    [SerializeField] private GameObject ArtifactUIPrefab;
    
    private List<ArtifactSlot> N_ArtifactSlots = new List<ArtifactSlot>();
    private List<ArtifactSlot> S_ArtifactSlots = new List<ArtifactSlot>();

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

        if (magneticController.magneticType == MagneticType.N)
        {
            foreach (var nArtifactSlot in N_ArtifactSlots)
            {
                nArtifactSlot.SetBackgroundColor(Color.red);
            }

            foreach (var sArtifactSlot in S_ArtifactSlots)
            {
                sArtifactSlot.SetBackgroundColor(Color.blue);
            }
        }
        else
        {
            foreach (var nArtifactSlot in N_ArtifactSlots)
            {
                nArtifactSlot.SetBackgroundColor(Color.blue);
            }

            foreach (var sArtifactSlot in S_ArtifactSlots)
            {
                sArtifactSlot.SetBackgroundColor(Color.red);
            }
        }
        
        if (artifactDataSO != null)
        {
            var artifactUI = Instantiate(ArtifactUIPrefab, artifactSlot.transform).GetComponent<ArtifactUI>();
            artifactUI.Initialized(artifactDataSO, transform);
        }
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    void Initialized()
    {
        for (int i = 0; i < 15; i++)
        {
            var instance1 = Instantiate(SlotPrefab, N_ArtifactInventory.transform).GetComponent<ArtifactSlot>();
            instance1.SetBackgroundColor(Color.red);
            instance1.SetSlotIndex(i);
            instance1.OnArtifactModified = UpdateArtifact_N;
            N_ArtifactSlots.Add(instance1);
            
            var instance2 = Instantiate(SlotPrefab, S_ArtifactInventory.transform).GetComponent<ArtifactSlot>();
            instance2.SetBackgroundColor(Color.blue);
            instance2.SetSlotIndex(i);
            instance2.OnArtifactModified = UpdateArtifact_S;
            S_ArtifactSlots.Add(instance2);
        }
    }

    void UpdateArtifact_N(int index, ArtifactDataSO artifact)
    {
        inventory.Left_ArtifactGas[index] = artifact;
    }
    
    void UpdateArtifact_S(int index, ArtifactDataSO artifact)
    {
        inventory.Right_ArtifactGas[index] = artifact;
    }
    
}
