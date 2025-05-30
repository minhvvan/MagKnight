using System;
using System.Collections;
using System.Collections.Generic;
using Highlighters;
using hvvan;
using UnityEngine;

public class MagneticHighlightController : MonoBehaviour
{
    [SerializeField] Highlighter _highlighterN;
    [SerializeField] Highlighter _highlighterS;

    private void Awake()
    {
        GameManager.Instance.OnMagneticPressed += OnMagneticPressed;
        GameManager.Instance.OnMagneticReleased += OnMagneticReleased;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnMagneticPressed -= OnMagneticPressed;
        GameManager.Instance.OnMagneticReleased -= OnMagneticReleased;
    }

    private void OnMagneticPressed()
    {
        _highlighterN.enabled = true;
        _highlighterS.enabled = true;
    }

    private void OnMagneticReleased()
    {
        _highlighterN.enabled = false;
        _highlighterS.enabled = false;
    }

    public void BindRenderer(GameObject magneticObject, MagneticType magneticType)
    {
        var objectRenderer = magneticObject.GetComponentInChildren<Renderer>();

        if (magneticType == MagneticType.N)
        {
            _highlighterN.Renderers.Add(new HighlighterRenderer(objectRenderer, 1));
        }
        else
        {
            _highlighterS.Renderers.Add(new HighlighterRenderer(objectRenderer, 1));
        }
    }

    public void UnbindRenderer(GameObject magneticObject, MagneticType magneticType)
    {
        var objectRenderer = magneticObject.GetComponentInChildren<Renderer>();
    
        // 동일한 Renderer를 갖는 HighlighterRenderer 객체 생성
        var highlighterToRemove = new HighlighterRenderer(objectRenderer, 1);
    
        if (magneticType == MagneticType.N)
        {
            _highlighterN.Renderers.Remove(highlighterToRemove);
        }
        else
        {
            _highlighterS.Renderers.Remove(highlighterToRemove);
        }
    }
}
