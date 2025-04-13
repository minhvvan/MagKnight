using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상호작용 가능 객체
/// </summary>
public interface IInteractable
{
    void Interact(IInteractor interactor);
    void Select();
    void UnSelect();
    GameObject GetGameObject();
}