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
    
    void Awake()
    {
        Initialized();
        inventory = FindObjectOfType<ArtifactInventory>();
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

    void UpdateArtifact_N(int index, ArtifactGAS artifact)
    {
        inventory.Left_ArtifactGas[index] = artifact;
    }
    
    void UpdateArtifact_S(int index, ArtifactGAS artifact)
    {
        inventory.Right_ArtifactGas[index] = artifact;
    }
    
}
