using System;
using DG.Tweening;
using hvvan;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Moon
{
    public class IntroUIController : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _pressAnyKeyText;
        [SerializeField] RectTransform _buttonRectTransform;
        [SerializeField] CanvasGroup _canvasGroup;
        [SerializeField] Button _continueButton;
        [SerializeField] Button _restartButton;
        [SerializeField] Button _creditButton;

        void Awake()
        {
            _continueButton.onClick.AddListener(OnClickContinue);
            _restartButton.onClick.AddListener(OnClickRestart);
            _creditButton.onClick.AddListener(OnClickCredit);

            _buttonRectTransform.gameObject.SetActive(false);
            _pressAnyKeyText.gameObject.SetActive(true);
        }

        private void OnClickContinue()
        {
            GameManager.Instance.ChangeGameState(GameState.InitGame);
        }

        private void OnClickRestart()
        {
            GameManager.Instance.DeleteData(Constants.CurrentRun);
            GameManager.Instance.ChangeGameState(GameState.InitGame);
        }

        private void OnClickCredit()
        {
            SceneManager.LoadScene("Credits");
        }

        void Update()
        {
            if(_pressAnyKeyText.gameObject.activeSelf)
            {
                if (Input.anyKeyDown)
                {
                    DOTween.KillAll();
                    _pressAnyKeyText.gameObject.SetActive(false);
                    _buttonRectTransform.gameObject.SetActive(true);

                    if(GameManager.Instance.CurrentRunData == null || !GameManager.Instance.CurrentRunData.isDungeonEnter)
                    {
                        _continueButton.gameObject.SetActive(false);
                    }

                    _canvasGroup.DOFade(1f, 0.5f);

                    //Cursor visible
                    UIManager.Instance.EnableCursor();
                }
            }
        }
    }
}