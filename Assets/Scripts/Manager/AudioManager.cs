using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource environmentSource;
    [SerializeField] private Queue<AudioSource> sfxSources = new Queue<AudioSource>();

    //SFX 풀링
    [SerializeField]private GameObject sfxPoolPrefab;
    [SerializeField]private Transform sfxContainer;
    [SerializeField]private int sfxPoolSize;
    
    // 오디오 클립들
    public AudioClip[] bgmClips;
    public AudioClip[] sfxClips;
    
    // 음소거 상태
    private bool bgmMuted = false;
    private bool sfxMuted = false;
    
    // 현재 볼륨 값 (0.0 ~ 1.0)
    private float bgmVolume = 1.0f;
    private float sfxVolume = 1.0f;
    
    private void Start()
    {
        LoadSettings();
    }

    private void OnDestroy()
    {
        DisposePooling();
    }

    private void LoadSettings()
    {
        // bgmVolume = PlayerPrefs.GetFloat(Constants.BGMVolume, 1.0f);
        // sfxVolume = PlayerPrefs.GetFloat(Constants.SFXVolume, 1.0f);
        // bgmMuted = PlayerPrefs.GetInt(Constants.BGMMute, 0) == 1;
        // sfxMuted = PlayerPrefs.GetInt(Constants.SFXMute, 0) == 1;
        
        // 볼륨 적용
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
        
        // 음소거 적용
        MuteBGM(bgmMuted);
        MuteSFX(sfxMuted);

        AudioSourcePooling();
    }

    #region SFX Pooling
    private void AudioSourcePooling()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            CreateSFXSources();
        }
    }
    
    private void DisposePooling()
    {
        while (sfxSources.Count > 0)
        {
            Destroy(sfxSources.Dequeue());
        }
    }
    
    private void CreateSFXSources()
    {
        var sfxObj =Instantiate(sfxPoolPrefab, sfxContainer);
        var sfx = sfxObj.GetComponent<AudioSource>();
        sfxSources.Enqueue(sfx);
    }
    
    private AudioSource GetSFXSources()
    {
        if (sfxSources.Count <= 0) CreateSFXSources();
        var sfxObj = sfxSources.Dequeue();
        return sfxObj;
    }

    public IEnumerator ReturnSFXSources(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        sfxSources.Enqueue(source);
    }
    #endregion
    
    #region SoundSetting
    // 배경음 볼륨 설정 (0.0 ~ 1.0)
    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        
        // 0 = -80
        float dbValue = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat(Constants.BGMVolume, dbValue);

        MuteBGM(volume == 0);

        // // 설정 저장
        // PlayerPrefs.SetFloat(Constants.BGMVolume, volume);
        // PlayerPrefs.Save();
    }
    
    // 효과음 볼륨 설정 (0.0 ~ 1.0)
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        
        float dbValue = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat(Constants.SFXVolume, dbValue);

        MuteSFX(volume == 0);
        
        // PlayerPrefs.SetFloat(Constants.SFXVolume, volume);
        // PlayerPrefs.Save();
    }
    
    public void MuteBGM(bool mute)
    {
        bgmMuted = mute;
        
        // 음소거 시 -80dB (거의 무음), 아닐 시 저장된 볼륨 적용
        float dbValue = mute ? -80f : (bgmVolume > 0.0001f ? Mathf.Log10(bgmVolume) * 20 : -80f);
        audioMixer.SetFloat(Constants.BGMVolume, dbValue);
        
        // PlayerPrefs.SetInt(Constants.BGMMute, mute ? 1 : 0);
        // PlayerPrefs.Save();
    }
    
    public void MuteSFX(bool mute)
    {
        sfxMuted = mute;
        
        float dbValue = mute ? -80f : (sfxVolume > 0.0001f ? Mathf.Log10(sfxVolume) * 20 : -80f);
        audioMixer.SetFloat(Constants.SFXVolume, dbValue);
        
        // PlayerPrefs.SetInt(Constants.SFXMute, mute ? 1 : 0);
        // PlayerPrefs.Save();
    }
    #endregion

    #region PlaySound
// 배경음악 재생
    public void PlayBGM(string clipName)
    {
        for (int i = 0; i < bgmClips.Length; i++)
        {
            if (bgmClips[i].name.Equals(clipName))
            {
                bgmSource.clip = bgmClips[i];
                bgmSource.loop = true;
                bgmSource.Play();
                return;
            }
        }
        Debug.LogWarning("BGM 클립을 찾을 수 없습니다: " + clipName);
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
    
    //환경음 재생
    public void PlayBGMEnvironment(string clipName)
    {
        for (int i = 0; i < bgmClips.Length; i++)
        {
            if (bgmClips[i].name.Equals(clipName))
            {
                environmentSource.clip = bgmClips[i];
                environmentSource.loop = true;
                environmentSource.Play();
                return;
            }
        }
        Debug.LogWarning("해당 클립을 찾을 수 없습니다: " + clipName);
    }
    
    public void StopBGMEnvironment()
    {
        environmentSource.Stop();
    }

    
    // 효과음 재생
    public void PlaySFX(string clipName)
    {
        AudioClip clip = null;
        for (int i = 0; i < sfxClips.Length; i++)
        {
            if (sfxClips[i].name.Equals(clipName))
            {
                clip = sfxClips[i];
                break;
            }
        }
    
        if (clip == null)
        {
            Debug.LogWarning("SFX 클립을 찾을 수 없습니다: " + clipName);
            return;
        }
    
        // 재생 가능한 효과음 소스 찾기
        var source = GetSFXSources();
            
        if (!source.isPlaying)
        {
            source.clip = clip;
            source.loop = false;
            source.Play();
            StartCoroutine(ReturnSFXSources(source));
        }
    }

    /// <summary>
    /// readonly List로 저장된 사운드를 랜덤하게 재생하고 싶을 때 사용합니다.
    /// </summary>
    /// <param name="clipNames"></param>
    /// <returns></returns>
    public string GetRandomClip(List<string> clipNames)
    {
        var range = clipNames.Count;
        var randomClip = clipNames[Random.Range(0, range)];

        return randomClip;
    }
    #endregion
}
