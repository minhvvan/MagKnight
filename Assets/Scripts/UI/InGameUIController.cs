using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIController : MonoBehaviour
{
    [SerializeField] FadeText entranceText;

    void Awake()
    {
        SceneTransitionEvent.OnSceneTransitionComplete += OnSceneTransitionComplete;
    }

    void OnEnable()
    {
        UIManager.Instance.SetInGameUIController(this);
    }

    private void OnSceneTransitionComplete(string entranceName, bool isSetNewName)
    {
       entranceText.SetTextAndShowFadeInAndOut(entranceName, isSetNewName);
    }
}