using System;
using System.Collections;
using System.Collections.Generic;
using Moon;
using UnityEngine;
using UnityEngine.InputSystem;
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

        void Start()
        {
            _inputHandler = FindObjectOfType<InputHandler>();  
        }
    
        public static void TransitionToScene(string sceneName)
        {
            Instance.StartCoroutine(Instance.Transition(sceneName));
        }

        protected IEnumerator Transition(string newSceneName)
        {
            _transitioning = true;

            if (_inputHandler == null)
                _inputHandler = FindObjectOfType<InputHandler>();
            if (_inputHandler) _inputHandler.ReleaseControl();
            yield return StartCoroutine(ScreenFader.FadeSceneOut(ScreenFader.FadeType.Loading));
            yield return SceneManager.LoadSceneAsync(newSceneName);
            _inputHandler = FindObjectOfType<InputHandler>();
            if (_inputHandler) _inputHandler.ReleaseControl();
            yield return StartCoroutine(ScreenFader.FadeSceneIn());
            if (_inputHandler)
                _inputHandler.GainControl();

            _transitioning = false;
        }

        IEnumerator CallWithDelay(float delay, Action call)
        {
            yield return new WaitForSeconds(delay);
            call();
        }
    }
}