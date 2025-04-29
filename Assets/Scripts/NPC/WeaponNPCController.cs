using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponNPCController : BaseNPCController
{
    [SerializeField] LootCrate lootCrate;

    protected void Start()
    {
        //랜덤 무기 생성
        lootCrate.SetLootCrate(ItemCategory.MagCore, ItemRarity.Common);
    }
    
    public override void Interact(IInteractor interactor)
    {
        base.Interact(interactor);
    }

    protected override void InteractEnter(IInteractor interactor)
    {
        base.InteractEnter(interactor);

    }

    protected override void InteractExit()
    {
        base.InteractExit();

        //상자 열기
        if (!lootCrate)
        {
            Debug.Log("Loot crate is null");
            return;
        }
        
        lootCrate.OpenCrate().Forget();
    }

    public new void Select()
    {
        
    }

    public new void UnSelect()
    {
        
    }
}
