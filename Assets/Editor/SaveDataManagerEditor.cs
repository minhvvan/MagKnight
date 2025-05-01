using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[CustomEditor(typeof(SaveDataManager))]
public class SaveDataManagerEditor : Editor
{
    private enum DataType
    {
        PlayerData,
        CurrentRunData
    }

    private DataType _selectedDataType = DataType.PlayerData;
    private string _rawJsonContent = string.Empty;
    private bool _isEditingEnabled = false;
    private Vector2 _jsonScrollPosition = Vector2.zero;
    private bool _isFormatted = false; // 현재 JSON이 포맷팅되었는지 여부
    private string _unformattedJson = string.Empty; // 포맷팅되지 않은 원본 JSON

    // 씬 변경 이벤트 리스너
    private static bool _isSceneChanging = false;

    // 텍스트 영역 스타일을 위한 GUIStyle
    private GUIStyle _jsonTextAreaStyle;
    private bool _stylesInitialized = false;

    // SaveDataManager 참조
    private SaveDataManager _saveDataManager;

    private void OnEnable()
    {
        // SaveDataManager 참조 가져오기
        _saveDataManager = (SaveDataManager)target;
        
        // 스타일 초기화는 OnInspectorGUI에서 처리
        // 씬 변경 이벤트 등록
        EditorSceneManager.sceneOpening += OnSceneOpening;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorSceneManager.sceneClosed += OnSceneClosed;
    }

    private void OnDisable()
    {
        // 이벤트 리스너 해제
        EditorSceneManager.sceneOpening -= OnSceneOpening;
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneClosed -= OnSceneClosed;
    }

    // 씬 열기 시작 이벤트 핸들러
    private void OnSceneOpening(string path, OpenSceneMode mode)
    {
        _isSceneChanging = true;
    }

    // 씬 열기 완료 이벤트 핸들러
    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        _isSceneChanging = false;
    }

    // 씬 닫기 이벤트 핸들러
    private void OnSceneClosed(Scene scene)
    {
        _isSceneChanging = true;
        // 지연 후 상태 복원
        EditorApplication.delayCall += () => {
            _isSceneChanging = false;
        };
    }

    // 스타일 초기화 함수
    private void InitializeStyles()
    {
        if (!_stylesInitialized && EditorStyles.textArea != null)
        {
            _jsonTextAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true  // 워드랩 활성화
            };
            _stylesInitialized = true;
        }
    }

    public override void OnInspectorGUI()
    {
        // 스타일 초기화 시도
        InitializeStyles();

        // 씬 전환 중에는 에디터 UI 비활성화
        if (_isSceneChanging)
        {
            EditorGUILayout.HelpBox("씬 전환 중에는 Save Data Editor를 사용할 수 없습니다.", MessageType.Info);
            return;
        }

        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Save Data Manager Tools", EditorStyles.boldLabel);
        
        // 데이터 타입 선택
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Data Type:", GUILayout.Width(100));
        _selectedDataType = (DataType)EditorGUILayout.EnumPopup(_selectedDataType);
        EditorGUILayout.EndHorizontal();
        
        // 삭제 버튼
        if (GUILayout.Button("Delete " + _selectedDataType.ToString()))
        {
            if (EditorUtility.DisplayDialog("Delete Save Data", 
                $"정말로 {_selectedDataType} 데이터를 삭제하시겠습니까?", 
                "예", "아니오"))
            {
                DeleteSelectedData();
            }
        }
        
        EditorGUILayout.Space();
        
        // 불러오기 버튼
        if (GUILayout.Button("Load " + _selectedDataType.ToString()))
        {
            LoadSelectedData();
            _isEditingEnabled = true;
            _isFormatted = false; // 로드 시 포맷팅 상태 초기화
        }
        
        // 데이터가 로드되었을 때만 표시
        if (!string.IsNullOrEmpty(_rawJsonContent))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("JSON Data:", EditorStyles.boldLabel);
            
            // 스크롤 시작
            _jsonScrollPosition = EditorGUILayout.BeginScrollView(_jsonScrollPosition, GUILayout.Height(300));
            
            // 스타일이 초기화되었는지 확인
            GUIStyle textAreaStyle = _stylesInitialized ? _jsonTextAreaStyle : EditorStyles.textArea;
            
            // 수정 가능한 텍스트 영역 (워드랩 적용)
            EditorGUI.BeginChangeCheck();
            string newContent = EditorGUILayout.TextArea(_rawJsonContent, textAreaStyle, GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
            {
                _rawJsonContent = newContent;
                if (_isFormatted)
                {
                    // 직접 편집 시 포맷팅 상태 해제
                    _isFormatted = false;
                }
            }
            
            // 스크롤 끝
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.BeginHorizontal();
            
            // 저장 버튼
            EditorGUI.BeginDisabledGroup(!_isEditingEnabled);
            if (GUILayout.Button("Save Changes"))
            {
                SaveSelectedData();
                EditorUtility.DisplayDialog("Save Data", "변경사항이 저장되었습니다.", "확인");
            }
            EditorGUI.EndDisabledGroup();
            
            // 포맷 토글 버튼
            string formatButtonText = _isFormatted ? "Compress JSON" : "Format JSON";
            if (GUILayout.Button(formatButtonText))
            {
                if (_isFormatted)
                {
                    // 압축 모드로 변환
                    CompressJSON();
                }
                else
                {
                    // 포맷팅 모드로 변환
                    FormatJSON();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
    
    private void LoadSelectedData()
    {
        string key = _selectedDataType.ToString();
        
        try
        {
            // SaveDataManager를 사용하여 데이터 로드
            // ISaveData를 직접 가져올 수 없으므로 실제 구현 클래스를 미리 알아야 함
            // 여기서는 임시로 PlayerData와 CurrentRunData 클래스가 있다고 가정
            
            // 키에 따라 다른 타입으로 로드
            var data = LoadDataByType(key);
            
            if (data != null)
            {
                // JsonUtility를 사용하여, ISaveData 인터페이스 구현체를 JSON 문자열로 변환
                _rawJsonContent = JsonUtility.ToJson(data);
                _unformattedJson = _rawJsonContent; // 원본 JSON 저장
                Debug.Log($"{key} 데이터를 성공적으로 불러왔습니다.");
            }
            else
            {
                Debug.LogWarning($"선택한 데이터 파일이 존재하지 않습니다: {key}");
                _rawJsonContent = string.Empty;
                EditorUtility.DisplayDialog("Warning", $"{key} 데이터 파일이 존재하지 않습니다.", "확인");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 로드 중 오류: {e.Message}");
            _rawJsonContent = string.Empty;
            EditorUtility.DisplayDialog("Error", $"데이터 로드 중 오류가 발생했습니다: {e.Message}", "확인");
        }
    }
    
    // 타입에 따라 적절한 데이터를 로드하는 헬퍼 메서드
    private ISaveData LoadDataByType(string key)
    {
        // 여기서는 간단한 예시를 위해 dynamic을 사용
        // 실제 환경에서는 타입에 따른 분기 처리 필요
        if (key == "PlayerData")
        {
            return _saveDataManager.LoadData<PlayerData>(key);
        }
        else if (key == "CurrentRunData")
        {
            return _saveDataManager.LoadData<CurrentRunData>(key);
        }
        
        return null;
    }
    
    private void SaveSelectedData()
    {
        string key = _selectedDataType.ToString();
        
        try
        {
            string jsonToSave = _rawJsonContent;
            
            // 포맷팅된 상태라면 압축 JSON으로 변환
            if (_isFormatted)
            {
                jsonToSave = CompressJsonString(_rawJsonContent);
            }
            
            // 저장 전 JSON 유효성 검사 시도
            try
            {
                // JSON 유효성 검사만 수행
                IsValidJson(jsonToSave);
            }
            catch (Exception)
            {
                if (!EditorUtility.DisplayDialog("Warning", 
                    "JSON 형식이 올바르지 않을 수 있습니다. 그래도 저장하시겠습니까?", 
                    "예", "아니오"))
                {
                    return;
                }
            }
            
            // SaveDataManager에 저장하기 위해 적절한 객체로 변환
            SaveData(key, jsonToSave);
            
            Debug.Log($"{key} 데이터를 성공적으로 저장했습니다.");
            
            // 저장 후 상태 업데이트
            _unformattedJson = jsonToSave;
            if (_isFormatted)
            {
                // 포맷팅된 상태를 유지하면서 저장한 경우, 내부적으로는 압축된 JSON 저장
                _rawJsonContent = FormatJsonString(jsonToSave); // 다시 포맷팅하여 표시
            }
            else
            {
                _rawJsonContent = jsonToSave;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 저장 중 오류: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"데이터 저장 중 오류가 발생했습니다: {e.Message}", "확인");
        }
    }
    
    // JSON 문자열을 적절한 객체로 변환하고 SaveDataManager를 통해 저장
    private async void SaveData(string key, string json)
    {
        if (key == "PlayerData")
        {
            var data = JsonUtility.FromJson<PlayerData>(json);
            await _saveDataManager.SaveData(key, data);
        }
        else if (key == "CurrentRunData")
        {
            var data = JsonUtility.FromJson<CurrentRunData>(json);
            await _saveDataManager.SaveData(key, data);
        }
    }
    
    private void DeleteSelectedData()
    {
        string key = _selectedDataType.ToString();
        
        try
        {
            // SaveDataManager를 사용하여 데이터 삭제
            _saveDataManager.DeleteData(key);
            
            Debug.Log($"{key} 데이터를 성공적으로 삭제했습니다.");
            _rawJsonContent = string.Empty;
            _unformattedJson = string.Empty;
            _isFormatted = false;
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 삭제 중 오류: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"데이터 삭제 중 오류가 발생했습니다: {e.Message}", "확인");
        }
    }
    
    // JSON 포맷팅 함수
    private void FormatJSON()
    {
        if (string.IsNullOrEmpty(_rawJsonContent))
            return;
            
        try
        {
            // 유효한 JSON인지 확인
            if (!IsValidJson(_rawJsonContent))
            {
                EditorUtility.DisplayDialog("Error", "유효하지 않은 JSON 형식입니다.", "확인");
                return;
            }
            
            // 원본 JSON 저장 (압축된 형태)
            if (!_isFormatted)
            {
                _unformattedJson = _rawJsonContent;
            }
            
            // 들여쓰기 추가
            string formattedJson = FormatJsonString(_rawJsonContent);
            _rawJsonContent = formattedJson;
            _isFormatted = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON 포맷팅 중 오류: {e.Message}");
            EditorUtility.DisplayDialog("Error", "JSON 포맷팅 중 오류가 발생했습니다.", "확인");
        }
    }
    
    // JSON 압축 함수 (포맷팅 제거)
    private void CompressJSON()
    {
        if (!_isFormatted || string.IsNullOrEmpty(_rawJsonContent))
            return;
            
        try
        {
            // 유효한 JSON인지 확인
            if (!IsValidJson(_rawJsonContent))
            {
                EditorUtility.DisplayDialog("Error", "유효하지 않은 JSON 형식입니다.", "확인");
                return;
            }
            
            // 원본 (압축된) JSON으로 복원하거나 새로 압축
            _rawJsonContent = !string.IsNullOrEmpty(_unformattedJson) 
                ? _unformattedJson 
                : CompressJsonString(_rawJsonContent);
                
            _isFormatted = false;
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON 압축 중 오류: {e.Message}");
            EditorUtility.DisplayDialog("Error", "JSON 압축 중 오류가 발생했습니다.", "확인");
        }
    }
    
    // JSON 문자열 형식이 유효한지 확인
    private bool IsValidJson(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
            return false;
            
        jsonString = jsonString.Trim();
        
        // JSON 객체나 배열로 시작하고 끝나는지 확인
        if ((jsonString.StartsWith("{") && jsonString.EndsWith("}")) || 
            (jsonString.StartsWith("[") && jsonString.EndsWith("]")))
        {
            try
            {
                // 간단한 문법 검사만 수행 (실제 파싱은 하지 않음)
                int openBraces = 0;
                int openBrackets = 0;
                bool inString = false;
                bool escaped = false;
                
                foreach (char c in jsonString)
                {
                    if (escaped)
                    {
                        escaped = false;
                        continue;
                    }
                    
                    if (c == '\\' && inString)
                    {
                        escaped = true;
                        continue;
                    }
                    
                    if (c == '"')
                    {
                        inString = !inString;
                        continue;
                    }
                    
                    if (inString)
                        continue;
                        
                    if (c == '{')
                        openBraces++;
                    else if (c == '}')
                        openBraces--;
                    else if (c == '[')
                        openBrackets++;
                    else if (c == ']')
                        openBrackets--;
                        
                    if (openBraces < 0 || openBrackets < 0)
                        return false;
                }
                
                return openBraces == 0 && openBrackets == 0 && !inString;
            }
            catch
            {
                return false;
            }
        }
        
        return false;
    }
    
    // JSON 문자열 포맷팅 (들여쓰기 추가)
    private string FormatJsonString(string json)
    {
        string indentString = "  ";
        int indentLevel = 0;
        bool inString = false;
        bool escaped = false;
        
        var sb = new System.Text.StringBuilder();
        
        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            
            if (escaped)
            {
                sb.Append(c);
                escaped = false;
                continue;
            }
            
            if (c == '\\' && inString)
            {
                sb.Append(c);
                escaped = true;
                continue;
            }
            
            if (c == '"')
            {
                sb.Append(c);
                inString = !inString;
                continue;
            }
            
            if (inString)
            {
                sb.Append(c);
                continue;
            }
            
            switch (c)
            {
                case '{':
                case '[':
                    sb.Append(c);
                    sb.Append('\n');
                    indentLevel++;
                    for (int j = 0; j < indentLevel; j++)
                        sb.Append(indentString);
                    break;
                    
                case '}':
                case ']':
                    sb.Append('\n');
                    indentLevel--;
                    for (int j = 0; j < indentLevel; j++)
                        sb.Append(indentString);
                    sb.Append(c);
                    break;
                    
                case ',':
                    sb.Append(c);
                    sb.Append('\n');
                    for (int j = 0; j < indentLevel; j++)
                        sb.Append(indentString);
                    break;
                    
                case ':':
                    sb.Append(c);
                    sb.Append(' ');
                    break;
                    
                default:
                    if (!char.IsWhiteSpace(c))
                        sb.Append(c);
                    break;
            }
        }
        
        return sb.ToString();
    }
    
    // JSON 문자열 압축 (포맷팅 제거)
    private string CompressJsonString(string json)
    {
        bool inString = false;
        bool escaped = false;
        
        var sb = new System.Text.StringBuilder();
        
        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            
            if (escaped)
            {
                sb.Append(c);
                escaped = false;
                continue;
            }
            
            if (c == '\\' && inString)
            {
                sb.Append(c);
                escaped = true;
                continue;
            }
            
            if (c == '"')
            {
                sb.Append(c);
                inString = !inString;
                continue;
            }
            
            if (inString)
            {
                sb.Append(c);
                continue;
            }
            
            // 문자열 외부에서는 공백 문자 무시
            if (!char.IsWhiteSpace(c))
            {
                sb.Append(c);
            }
        }
        
        return sb.ToString();
    }
}
#endif