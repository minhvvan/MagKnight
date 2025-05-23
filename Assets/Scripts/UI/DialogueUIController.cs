using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using KoreanTyper;

public class DialogueUIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _dialogueText;
    [SerializeField] Image _nextImage;

    float _typingProcess = 0f;
    float _typingSpeed = 0.01f;

    string _text = string.Empty;

    Coroutine _typingCoroutine;
    public bool IsTyping => _typingCoroutine != null;

    DialogueDataSO _currentDialogueData = null;
    
    private AudioSource _typingLoopSrc; 

    int _currentDialogueIndex = 0;
    public int CurrentDialogueIndex => _currentDialogueIndex;

    void OnEnable()
    {
        InteractionEvent.OnDialogueEnd += HideDialogue;
    }

    void OnDisable()
    {
        _currentDialogueData = null;
        InteractionEvent.OnDialogueEnd -= HideDialogue;
    }

    void Update()
    {
        //테스트
        if (Input.anyKeyDown)
        {
            if (IsTyping)
            {
                SkipTyping();
            }
            else
            {
                NextDialogue();
            }
        }
    }

    public void SetName(string name)
    {
        _nameText.text = name;
    }
    
    public void SetText(string dialogue)
    {
        _text = dialogue;

        if(_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        _typingCoroutine = StartCoroutine(TypingText());
    }

    public void SetTypingSpeed(float speed)
    {
        _typingSpeed = speed;
    }

    public void SetTypingProcess(float process)
    {
        _typingProcess = process;
    }
    
    // 대화 출력
    IEnumerator TypingText() 
    {   
        _typingLoopSrc = AudioManager.Instance.PlayLoopSFX(AudioBase.SFX.NPC.Talk[0]);
        for (int i = 0; i <= _text.GetTypingLength(); i++) {
            _dialogueText.text = _text.Typing(i);
            yield return new WaitForSeconds(_typingSpeed);
        }

        AudioManager.Instance.StopLoopSFX(_typingLoopSrc);
        _typingLoopSrc = null;

        _nextImage.gameObject.SetActive(true);
    }

    // 즉시 전체 출력
    public void SkipTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        AudioManager.Instance.StopLoopSFX(_typingLoopSrc);
        _typingLoopSrc = null;

        _dialogueText.text = _text;
        _nextImage.gameObject.SetActive(true);
    }

    // 대화창 종료
    public void HideDialogue()
    {
        _dialogueText.text = string.Empty;
        _nameText.text = string.Empty;
        _nextImage.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ShowDialogue(DialogueDataSO dialogueData)
    {
        
        _currentDialogueData = dialogueData;
        _currentDialogueIndex = 0;
        gameObject.SetActive(true);
        SetName(_currentDialogueData.npcName);
        if (_currentDialogueData.lines.Count == 0)
        {
            SetText("");            
        }
        else
        {
            SetText(_currentDialogueData.lines[0].text);
        }
        
        
        _dialogueText.text = string.Empty;
    }

    public void NextDialogue()
    {
        if (_nextImage.gameObject.activeSelf)
        {
            _nextImage.gameObject.SetActive(false);
        }

        if (_currentDialogueIndex < _currentDialogueData.lines.Count - 1)
        {
            _currentDialogueIndex++;
            SetText(_currentDialogueData.lines[_currentDialogueIndex].text);
        }
        else
        {
            //대화 종료
            InteractionEvent.DialogueEnd();
        }
    }
}
