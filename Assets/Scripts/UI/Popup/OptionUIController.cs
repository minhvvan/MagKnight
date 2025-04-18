using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionUIController : MonoBehaviour, IBasePopupUIController
{
    [SerializeField] Slider _mainSlider;
    [SerializeField] Slider _bgmSlider;
    [SerializeField] Slider _sfxSlider;
    [SerializeField] Button _closeButton;

    void Awake()
    {
        _closeButton.onClick.AddListener(OnClickClose);

        _mainSlider.onValueChanged.AddListener(OnMainSliderValueChanged);
        _bgmSlider.onValueChanged.AddListener(OnBGMVolumeSliderValueChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXVolumeSliderValueChanged);
    }

    void OnClickClose()
    {
        HideUI();
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
    }

    public void HideUI()
    {
        gameObject.SetActive(false);
    }

    //Slider value changed event
    public void OnMainSliderValueChanged(float value)
    {
        // Set the main volume in the audio manager
        //AudioManager.Instance.SetMainVolume(value);
    }

    public void OnBGMVolumeSliderValueChanged(float value)
    {
        // Set the BGM volume in the audio manager
        //AudioManager.Instance.SetBGMVolume(value);
    }

    public void OnSFXVolumeSliderValueChanged(float value)
    {
        // Set the SFX volume in the audio manager
        //AudioManager.Instance.SetSFXVolume(value);
    }
}

