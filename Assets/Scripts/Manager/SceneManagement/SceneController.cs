using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Moon
{
    public class SceneController : Singleton<SceneController>
    {
        //기존 샘플 코드에서는 TriggerEvent의 중복을 막기 위하여 사용했던것으로 보임
        public static bool Transitioning
        {
            get { return Instance._transitioning; }
        }
          
        protected InputHandler _inputHandler;
        protected bool _transitioning;

        bool _sceneReady = false;

        void Start()
        {
            _inputHandler = FindObjectOfType<InputHandler>();  
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _sceneReady = true;
        }
    
        public static void TransitionToScene(string sceneName)
        {
            Instance.StartCoroutine(Instance.Transition(sceneName, ScreenFader.FadeType.Loading, 1f, true));
            // Instance.StartCoroutine(Instance.Transition(sceneName, ScreenFader.FadeType.CommonFade));
        }

        protected IEnumerator Transition(string newSceneName, ScreenFader.FadeType fadeType = ScreenFader.FadeType.Loading, float lodingDelay = 1f, bool isStopTimeScale = false)
        {
            _transitioning = true;

            if (_inputHandler == null)
                _inputHandler = FindObjectOfType<InputHandler>();
            if (_inputHandler) _inputHandler.ReleaseControl();

            if (isStopTimeScale)
                Time.timeScale = 0f;

            yield return StartCoroutine(ScreenFader.FadeSceneOut(fadeType));

            _sceneReady = false;
            SceneManager.sceneLoaded += OnSceneLoaded;
            yield return SceneManager.LoadSceneAsync(newSceneName);

            yield return new WaitUntil(() => _sceneReady);
            yield return new WaitForSecondsRealtime(lodingDelay);
            _inputHandler = FindObjectOfType<InputHandler>();
            if (_inputHandler) _inputHandler.ReleaseControl();

            if (isStopTimeScale)
                Time.timeScale = 1f;

            yield return StartCoroutine(ScreenFader.FadeSceneIn());

            if (_inputHandler)
                _inputHandler.GainControl();

            _transitioning = false;
        }
    }
}