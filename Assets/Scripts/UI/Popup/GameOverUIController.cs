using System;
using System.Collections;
using System.Collections.Generic;
using hvvan;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Moon;

public class GameOverUIController : MonoBehaviour, IBasePopupUIController
{
    [SerializeField] RectTransform _gameOverTextTransform;
    [SerializeField] Button _restartButton;
    [SerializeField] Button _exitButton;
    [SerializeField] CanvasGroup _backgroundCanvasGroup;
    [SerializeField] CanvasGroup _buttonCanvasGroup;
    [SerializeField] float _fadeDuration = 0.5f;

    void Awake()
    {
        _restartButton.onClick.AddListener(OnClickRestart);
        _exitButton.onClick.AddListener(OnClickExit);

        Initialize();
    }

    void Initialize()
    {
        _backgroundCanvasGroup.alpha = 0;
        _buttonCanvasGroup.alpha = 0;
        
        _gameOverTextTransform.anchoredPosition = new Vector2(0,0);
    }


    void AnimateShowUI()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_backgroundCanvasGroup.DOFade(1, _fadeDuration))
                .Append(_gameOverTextTransform.DOLocalMoveY(200, _fadeDuration))
                .Append(_buttonCanvasGroup.DOFade(1, _fadeDuration));
    }

    void AnimateHideUI()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_backgroundCanvasGroup.DOFade(0, _fadeDuration)).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }



    void OnClickRestart()
    {
        SceneController.TransitionToScene(Constants.BaseCamp);
        GameManager.Instance.ChangeGameState(GameState.BaseCamp);
    }

    void OnClickExit()
    {
        Application.Quit();
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
        Initialize();
        AnimateShowUI();
    }

    public void HideUI()
    {
        AnimateHideUI();
    }
}
