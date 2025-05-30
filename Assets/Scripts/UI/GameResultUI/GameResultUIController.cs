using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using hvvan;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUIController : MonoBehaviour, IBasePopupUIController
{
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text playTimeText;
    [SerializeField] TMP_Text clearedFloorText;
    [SerializeField] TMP_Text clearedRoomText;
    [SerializeField] TMP_Text opponentsDefeatedText;
    [SerializeField] TMP_Text artifactCountText;
    [SerializeField] TMP_Text currencyText;
    [SerializeField] GameObject artifactsPanel;
    [SerializeField] Image clearedImage;
    [SerializeField] Button restartButton;
    [SerializeField] GameObject artifactImageObj;
    [SerializeField] TMP_Text gameOverText;
    
    [SerializeField] private List<Sprite> resultImage;

    
    [SerializeField] CanvasGroup backgroundCanvasGroup;
    [SerializeField] RectTransform resultUIRectTransform;
    [SerializeField] RectTransform gameOverTextRectTransform;
    
    private int _currency;
    
    void Start()
    {
        restartButton.onClick.AddListener(OnClickButton);
    }
    
    public async void ShowUI(CurrentRunData currentRunData, GameResult gameResult)
    {
        if (gameResult == GameResult.GameOver)
        {
            resultText.text = "죽었습니다";
            gameOverText.text = "게임 오버";
            gameOverText.color = Color.red;
            clearedImage.sprite = resultImage[0];
        }
        else if (gameResult == GameResult.GameClear)
        {
            resultText.text = "클리어하였습니다";
            gameOverText.text = "게임 클리어";
            clearedImage.sprite = resultImage[1];
            currentRunData.currentFloor++;
        }
        int totalSeconds = Mathf.FloorToInt(currentRunData.playTime);
        
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        
        playTimeText.SetText($"[ {hours:00}:{minutes:00}:{seconds:00} ]");
        clearedFloorText.SetText(currentRunData.currentFloor.ToString());
        
        int clearedRooms = currentRunData.clearedRoomsCount - 1; //시작 방 제외
        
        clearedRoomText.SetText(clearedRooms.ToString());
        opponentsDefeatedText.SetText(currentRunData.opponentsDefeated.ToString());
        artifactCountText.SetText(currentRunData.artifactsId.Count.ToString());
        
        var artifactData = await DataManager.Instance.LoadScriptableObjectAsync<ArtifactDataMappingSO>(Addresses.Data.Artifact.ArtifactMappingData);

        foreach (var artifactId in currentRunData.artifactsId)
        {
            var artifactImg = Instantiate(artifactImageObj, artifactsPanel.transform);
            artifactImg.GetComponent<Image>().sprite = artifactData.artifacts[artifactId].icon;
        }
        
        _currency = currentRunData.artifactsId.Count * 10 + currentRunData.currentFloor * 50 + clearedRooms * 10 + currentRunData.opponentsDefeated * 5;
        currencyText.SetText(_currency.ToString());
        
        var playerData = await GameManager.Instance.GetPlayerData();
        playerData.Currency = _currency;
        _ = GameManager.Instance.SetPlayerData(playerData);
        
        ShowUI();
    }
    
    public void ShowUI()
    {
        GameManager.Instance.Player.InputHandler.ReleaseControl();
        UIManager.Instance.inGameUIController.HideInGameUI();
        gameObject.SetActive(true);
        AnimateShowUI();
    }

    public void HideUI()
    {
        AnimateHideUI();
        GameManager.Instance.Player.InputHandler.GainControl();
    }

    private async void OnClickButton()
    {
        //데이터 관리(current삭제 및 재화 저장)
        GameManager.Instance.DeleteData(Constants.CurrentRun);
        await GameManager.Instance.SaveData(Constants.PlayerData);
        //베이스 캠프로 이동
        GameManager.Instance.ChangeGameState(GameState.InitGame);
        
        HideUI();
    }
    
    void AnimateShowUI()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(backgroundCanvasGroup.DOFade(1f, 0.5f))
            .Append(gameOverTextRectTransform.DOScale(1, 0.5f)).OnComplete(async () =>
            {
                await UniTask.Delay(1500);
                resultUIRectTransform.DOScale(1, 0.5f);
            });
    }

    void AnimateHideUI()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(backgroundCanvasGroup.DOFade(0, 0.5f)).OnComplete(() =>
        {
            gameObject.SetActive(false);
            resultUIRectTransform.localScale = Vector3.zero;
            gameOverTextRectTransform.localScale = Vector3.zero;
        });
    }
}

public enum GameResult
{
    GameOver,
    GameClear
}