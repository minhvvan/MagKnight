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
    private float masterVolume = 0.0f;
    private float bgmVolume = 0.0f;
    private float sfxVolume = 0.0f;
    
    
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
        bgmVolume = PlayerPrefs.GetFloat(Constants.BGMVolume, 0.5f);
        sfxVolume = PlayerPrefs.GetFloat(Constants.SFXVolume, 0.5f);
        masterVolume = PlayerPrefs.GetFloat(Constants.MasterVolume, 0.5f);
        
        // 볼륨 적용
        SetMasterVolume(masterVolume);
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
    

    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(Constants.MasterVolume, 0.5f);
    }

    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat(Constants.BGMVolume, 0.5f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(Constants.SFXVolume, 0.5f);
    }

    #region SoundSetting

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        
        // 0 = -80
        float dbValue = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat(Constants.MasterVolume, dbValue);
        
        PlayerPrefs.SetFloat(Constants.MasterVolume, volume);
        PlayerPrefs.Save();
    }

    // 배경음 볼륨 설정 (0.0 ~ 1.0)
    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        
        // 0 = -80
        float dbValue = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat(Constants.BGMVolume, dbValue);

        MuteBGM(volume == 0);

        // // 설정 저장
        PlayerPrefs.SetFloat(Constants.BGMVolume, volume);
        PlayerPrefs.Save();
    }
    
    // 효과음 볼륨 설정 (0.0 ~ 1.0)
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        
        float dbValue = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat(Constants.SFXVolume, dbValue);

        MuteSFX(volume == 0);
        
        PlayerPrefs.SetFloat(Constants.SFXVolume, volume);
        PlayerPrefs.Save();
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
    
    public AudioSource PlayLoopSFX(string clipName)
    { 
        AudioClip clip = Array.Find(sfxClips, c => c.name == clipName); 
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] SFX not found: {clipName}");
            return null;
        }
        
        var src = GetSFXSources(); 
        src.clip  = clip; 
        src.loop  = true; 
        src.Play(); 
        return src;                     // 호출 측에서 Stop 시켜야 함
    }
    /// <summary>루프 SFX 정지 후 풀에 반납.</summary>
    public void StopLoopSFX(AudioSource src)
    { 
        if (src == null) return; 
        src.Stop(); 
        src.loop = false; 
        StartCoroutine(ReturnSFXSources(src)); 
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
