using System.Collections;
using System.Collections.Generic;
using hvvan;
using TMPro;
using UnityEngine;

public class CurrencyUIController : MonoBehaviour
{
    [SerializeField] TMP_Text currencyText;
    [SerializeField] TMP_Text scrapText;


    public void InitializeCurrencyUI(PlayerData playerData)
    {
        currencyText.text = playerData.Currency.ToString();
        scrapText.text = 0.ToString();
        if (GameManager.Instance.CurrentRunData is { } currentRunData)
        {
            scrapText.text = currentRunData.scrap.ToString();
        }
    }
    
    public void UpdateScrap()
    {
        var currentRunData = GameManager.Instance.CurrentRunData;
        int.TryParse(scrapText.text, out int prevScrap);
        var newScrap = currentRunData.scrap;
        StartCoroutine(UpdateScrapAnim(prevScrap, newScrap));
    }

    public void UpdateCurrency(PlayerData playerData)
    {
        currencyText.text = playerData.Currency.ToString();
    }

    private IEnumerator UpdateScrapAnim(int prevScrap, int newScrap, float duration = 0.5f)
    {
        var currentScrap = prevScrap;
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currentScrap = (int)Mathf.Lerp(currentScrap, newScrap, elapsedTime / duration);
            scrapText.text = currentScrap.ToString();
            yield return null;
        }
        scrapText.text = newScrap.ToString();
    }
}
