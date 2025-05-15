using System;
using System.Collections;
using System.Collections.Generic;
using hvvan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUIController : MonoBehaviour, IBasePopupUIController
{
    [SerializeField] Slider _mainSlider;
    [SerializeField] Slider _bgmSlider;
    [SerializeField] Slider _sfxSlider;
    [SerializeField] Slider _mouseXAxisSlider;
    [SerializeField] Slider _mouseYAxisSlider;

    [SerializeField] Button _closeButton;


    [SerializeField] TextMeshProUGUI _mainVolumeText;
    [SerializeField] TextMeshProUGUI _bgmVolumeText;
    [SerializeField] TextMeshProUGUI _sfxVolumeText;
    [SerializeField] TextMeshProUGUI _mouseXAxisText;
    [SerializeField] TextMeshProUGUI _mouseYAxisText;

    void Awake()
    {
        _closeButton.onClick.AddListener(OnClickClose);

        _mainSlider.onValueChanged.AddListener(OnMainSliderValueChanged);
        _bgmSlider.onValueChanged.AddListener(OnBGMVolumeSliderValueChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXVolumeSliderValueChanged);

        _mouseXAxisSlider.onValueChanged.AddListener(OnMouseXAxisSliderValueChanged);
        _mouseYAxisSlider.onValueChanged.AddListener(OnMouseYAxisSliderValueChanged);
    }
    

    void OnEnable()
    {
        // Set the sliders to the current volume levels
        _mainSlider.value = AudioManager.Instance.GetMasterVolume();
        _bgmSlider.value = AudioManager.Instance.GetBGMVolume();
        _sfxSlider.value = AudioManager.Instance.GetSFXVolume();
        _mouseXAxisSlider.value = GameManager.Instance.Player.cameraSettings.GetMouseXSensitivity();
        _mouseYAxisSlider.value = GameManager.Instance.Player.cameraSettings.GetMouseYSensitivity();
        

        // Update the text labels with the current volume levels
        _mainVolumeText.text = $"{Mathf.RoundToInt(_mainSlider.value * 100)}";
        _bgmVolumeText.text = $"{Mathf.RoundToInt(_bgmSlider.value * 100)}";
        _sfxVolumeText.text = $"{Mathf.RoundToInt(_sfxSlider.value * 100)}";
        _mouseXAxisText.text = $"{_mouseXAxisSlider.value:F1}";
        _mouseYAxisText.text = $"{_mouseYAxisSlider.value:F1}";
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
        AudioManager.Instance.SetMasterVolume(value);
        _mainVolumeText.text = $"{Mathf.RoundToInt(value * 100)}";
    }

    public void OnBGMVolumeSliderValueChanged(float value)
    {
        // Set the BGM volume in the audio manager
        AudioManager.Instance.SetBGMVolume(value);
        _bgmVolumeText.text = $"{Mathf.RoundToInt(value * 100)}";        
    }

    public void OnSFXVolumeSliderValueChanged(float value)
    {
        // Set the SFX volume in the audio manager
        AudioManager.Instance.SetSFXVolume(value);
        _sfxVolumeText.text = $"{Mathf.RoundToInt(value * 100)}";        
    }

    private void OnMouseYAxisSliderValueChanged(float value)
    {
        GameManager.Instance.Player.cameraSettings.SetMouseYSensitivity(value);
        _mouseYAxisText.text = $"{value:F1}";
    }

    private void OnMouseXAxisSliderValueChanged(float value)
    {
        GameManager.Instance.Player.cameraSettings.SetMouseXSensitivity(value);
        _mouseXAxisText.text = $"{value:F1}";        
    }
}

