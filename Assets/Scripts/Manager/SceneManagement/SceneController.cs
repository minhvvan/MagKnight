using System;
using System.Collections;
using hvvan;
using Managers;
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
        
        private SceneMappingSO _sceneMapping;

        bool _sceneReady = false;

        async void  Start()
        {
            _inputHandler = FindObjectOfType<InputHandler>();
            _sceneMapping = await DataManager.Instance.LoadScriptableObjectAsync<SceneMappingSO>(Addresses.Data.Common.SceneData);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _sceneReady = true;
        }
    
        public static void TransitionToScene(string sceneName, bool showSceneTitle = true, Func<IEnumerator> sceneLoadedAction = null)
        {
            Instance.StartCoroutine(Instance.Transition(sceneName, ScreenFader.FadeType.Loading, 1f, true, showSceneTitle, sceneLoadedAction));
            // Instance.StartCoroutine(Instance.Transition(sceneName, ScreenFader.FadeType.CommonFade));
        }

        protected IEnumerator Transition(string newSceneName, ScreenFader.FadeType fadeType = ScreenFader.FadeType.Loading, float loadingDelay = 1f, bool isStopTimeScale = false, bool showSceneTitle = true, Func<IEnumerator> sceneLoadedAction = null)
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
            yield return new WaitForSecondsRealtime(loadingDelay);
            
            // 콜백이 있는 경우 실행하고 완료될 때까지 대기
            if (sceneLoadedAction != null)
            {
                yield return StartCoroutine(sceneLoadedAction());
            }
            
            _inputHandler = FindObjectOfType<InputHandler>();
            if (_inputHandler) _inputHandler.ReleaseControl();

            if (isStopTimeScale)
                Time.timeScale = 1f;

            yield return StartCoroutine(ScreenFader.FadeSceneIn());

            if (_inputHandler)
                _inputHandler.GainControl();


            if (showSceneTitle)
            {
                var title = "";
                if (_sceneMapping)
                {
                    title = _sceneMapping.scenes[newSceneName].sceneTitle;
                }
                SceneTransitionEvent.TriggerSceneTransitionComplete(title, true);
            }

            _transitioning = false;
        }
    }
}