using System.Collections;
using System.Collections.Generic;
using hvvan;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUIController : MonoBehaviour
{
    [SerializeField] Button _resumeButton;
    [SerializeField] Button _giveUpButton;
    [SerializeField] Button _optionButton;
    [SerializeField] Button _exitButton;
    

    void Awake()
    {
        _resumeButton.onClick.AddListener(OnClickResume);
        _giveUpButton.onClick.AddListener(OnClickGiveUp);
        _optionButton.onClick.AddListener(OnClickOption);
        _exitButton.onClick.AddListener(OnClickExit);  
    }

    void OnEnable()
    {
        UIManager.Instance.ShowBackgroundImage(true);
        
    }

    void OnDisable()
    {
        UIManager.Instance.ShowBackgroundImage(false);
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
    }

    public void HideUI()
    {
        gameObject.SetActive(false);
    }


    void OnClickResume()
    {
        GameManager.Instance.RecoverPreviousState();
    }

    void OnClickGiveUp()
    {
        UIManager.Instance.ShowConfirmPopup("포기", "베이스캠프로 돌아가시겠습니까?", () =>
        {
            GameManager.Instance.ChangeGameState(GameState.InitGame);
        }, null, true);
    }

    void OnClickOption()
    {
        UIManager.Instance.ShowOptionUI();
    }

    void OnClickExit()
    {
        UIManager.Instance.ShowConfirmPopup("나가기", "정말로 나가시겠습니까?", () =>
        {
            
            //Play Mode exit
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
            //나중에 GameManager에서 베이스 캠프 나가는것 처럼 해도 괜찮을듯?
        }, null, true);
    }
}
