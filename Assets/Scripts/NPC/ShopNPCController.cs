using System;
using System.Collections;
using System.Collections.Generic;
using hvvan;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShopNPCController : BaseNPCController
{
    //아이템 포장박스
    [SerializeField] private GameObject productCasePrefab;
    [SerializeField] private Transform[] createPoints;

    private HashSet<ArtifactDataSO> artifactsItem = new HashSet<ArtifactDataSO>();
    
    protected override void Awake()
    {
        base.Awake();
        ItemDisplay();
    }
    
    protected override void InteractExit()
    {
        base.InteractExit();
    }

    //상인 앞에 아이템 진열
    private void ItemDisplay()
    {
        artifactsItem.Clear();
        for (int i = 0; i < createPoints.Length; i++)
        {
            var caseObj = Instantiate(productCasePrefab, createPoints[i].position, 
                Quaternion.identity, createPoints[i]);
            caseObj.transform.forward = transform.forward;
            var productCase = caseObj.GetComponent<ProductCase>();
            if(productCase == null)
            {
                Debug.LogError("ProductCase component not found on the prefab.");
                continue;
            }
            
            //아티팩트
            if (i >= 0 && i <= 2)
            {
                var artifactObj = ItemManager.Instance.CreateItem(ItemCategory.Artifact, RaritySelector.GetRandomRarity(),
                    createPoints[i].position,Quaternion.identity);
                
                if (artifactObj == null || !artifactsItem.Add(artifactObj.GetComponent<ArtifactObject>().GetArtifactData()))
                {
                    caseObj.Destroy();
                    continue;
                }
                productCase.Casing(artifactObj);
            }
            
            //파츠
            if (i >= 3 && i <= 4)
            {
                var magCoreObj = ItemManager.Instance.CreateItem(ItemCategory.MagCore,RaritySelector.GetRandomRarity(),
                    createPoints[i].position,Quaternion.identity);
                if (magCoreObj == null)
                {
                    caseObj.Destroy();
                    continue;
                }

                productCase.Casing(magCoreObj);
            }
            
            //포션
            if (i >= 5 && i <= 6)
            {
                var healthPackObj = ItemManager.Instance.CreateItem(ItemCategory.HealthPack,ItemRarity.Common,
                    createPoints[i].position,Quaternion.identity);

                if (healthPackObj == null)
                {
                    caseObj.Destroy();
                    continue;
                }
                productCase.Casing(healthPackObj);
            }
            
        }
    }
}
