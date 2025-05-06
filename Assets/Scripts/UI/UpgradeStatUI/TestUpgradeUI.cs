using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUpgradeUI : MonoBehaviour, IInteractable
{
    public void Interact(IInteractor interactor)
    {
        UIManager.Instance.ShowUpgradeStatUI();
    }

    public void Select()
    {
    }

    public void UnSelect()
    {
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
