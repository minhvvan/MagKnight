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

    NPCSO _currentNPCSO = null;

    int _currentDialogueIndex = 0;
    public int CurrentDialogueIndex => _currentDialogueIndex;

    void OnEnable()
    {
        InteractionEvent.OnDialogueEnd += HideDialogue;
    }

    void OnDisable()
    {
        _currentNPCSO = null;
        InteractionEvent.OnDialogueEnd -= HideDialogue;
    }

    void Update()
    {
        //테스트
        if (Input.GetKeyDown(KeyCode.Space))
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
        for (int i = 0; i <= _text.GetTypingLength(); i++) {
            _dialogueText.text = _text.Typing(i);
            yield return new WaitForSeconds(_typingSpeed);
        }

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

    public void ShowDialogue(NPCSO npcSO)
    {
        _currentNPCSO = npcSO;
        _currentDialogueIndex = 0;
        gameObject.SetActive(true);
        SetName(_currentNPCSO.npcName);
        if (_currentNPCSO.dialogueData.lines.Count == 0)
        {
            SetText("");            
        }
        else
        {
            SetText(_currentNPCSO.dialogueData.lines[0].text);
        }
        
        _dialogueText.text = string.Empty;
    }

    public void NextDialogue()
    {
        if (_nextImage.gameObject.activeSelf)
        {
            _nextImage.gameObject.SetActive(false);
        }

        if (_currentDialogueIndex < _currentNPCSO.dialogueData.lines.Count - 1)
        {
            _currentDialogueIndex++;
            SetText(_currentNPCSO.dialogueData.lines[_currentDialogueIndex].text);
        }
        else
        {
            //대화 종료
            InteractionEvent.DialogueEnd();
        }
    }
}
