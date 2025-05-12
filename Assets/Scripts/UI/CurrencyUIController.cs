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
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        var currentRunData = GameManager.Instance.CurrentRunData;
        scrapText.text = currentRunData.scrap.ToString();
    }
}
