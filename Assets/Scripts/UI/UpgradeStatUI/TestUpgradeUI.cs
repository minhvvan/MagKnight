using System.Collections;
using System.Collections.Generic;
using Highlighters;
using UnityEngine;

public class TestUpgradeUI : MonoBehaviour, IInteractable
{
    public void Interact(IInteractor interactor)
    {
        UIManager.Instance.ShowUpgradeStatUI();
    }

    public void Select(Highlighter highlighter)
    {
    }

    public void UnSelect(Highlighter highlighter)
    {
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
