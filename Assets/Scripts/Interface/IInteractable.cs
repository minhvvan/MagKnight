using Highlighters;
using UnityEngine;

/// <summary>
/// 상호작용 가능 객체
/// </summary>
public interface IInteractable
{
    void Interact(IInteractor interactor);
    void Select(Highlighter highlighter);
    void UnSelect(Highlighter highlighter);
    GameObject GetGameObject();
    InteractType GetInteractType();
}