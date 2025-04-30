using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[CustomEditor(typeof(SaveDataManager))]
public class SimpleSaveDataEditor : Editor
{
    private enum DataType
    {
        PlayerData,
        CurrentRunData
    }

    private DataType selectedDataType = DataType.PlayerData;
    private string rawJsonContent = string.Empty;
    private bool isEditingEnabled = false;
    private Vector2 jsonScrollPosition = Vector2.zero;
    private bool isFormatted = false; // 현재 JSON이 포맷팅되었는지 여부
    private string unformattedJson = string.Empty; // 포맷팅되지 않은 원본 JSON

    // 씬 변경 이벤트 리스너
    private static bool isSceneChanging = false;

    // 텍스트 영역 스타일을 위한 GUIStyle
    private GUIStyle jsonTextAreaStyle;
    private bool stylesInitialized = false;

    private void OnEnable()
    {
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
        isSceneChanging = true;
    }

    // 씬 열기 완료 이벤트 핸들러
    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        isSceneChanging = false;
    }

    // 씬 닫기 이벤트 핸들러
    private void OnSceneClosed(Scene scene)
    {
        isSceneChanging = true;
        // 지연 후 상태 복원
        EditorApplication.delayCall += () => {
            isSceneChanging = false;
        };
    }

    // 스타일 초기화 함수
    private void InitializeStyles()
    {
        if (!stylesInitialized && EditorStyles.textArea != null)
        {
            jsonTextAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true  // 워드랩 활성화
            };
            stylesInitialized = true;
        }
    }

    public override void OnInspectorGUI()
    {
        // 스타일 초기화 시도
        InitializeStyles();

        // 씬 전환 중에는 에디터 UI 비활성화
        if (isSceneChanging)
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
        selectedDataType = (DataType)EditorGUILayout.EnumPopup(selectedDataType);
        EditorGUILayout.EndHorizontal();
        
        // 삭제 버튼
        if (GUILayout.Button("Delete " + selectedDataType.ToString()))
        {
            if (EditorUtility.DisplayDialog("Delete Save Data", 
                $"정말로 {selectedDataType} 데이터를 삭제하시겠습니까?", 
                "예", "아니오"))
            {
                DeleteSelectedData();
            }
        }
        
        EditorGUILayout.Space();
        
        // 불러오기 버튼
        if (GUILayout.Button("Load " + selectedDataType.ToString()))
        {
            LoadSelectedData();
            isEditingEnabled = true;
            isFormatted = false; // 로드 시 포맷팅 상태 초기화
        }
        
        // 데이터가 로드되었을 때만 표시
        if (!string.IsNullOrEmpty(rawJsonContent))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("JSON Data:", EditorStyles.boldLabel);
            
            // 스크롤 시작
            jsonScrollPosition = EditorGUILayout.BeginScrollView(jsonScrollPosition, GUILayout.Height(300));
            
            // 스타일이 초기화되었는지 확인
            GUIStyle textAreaStyle = stylesInitialized ? jsonTextAreaStyle : EditorStyles.textArea;
            
            // 수정 가능한 텍스트 영역 (워드랩 적용)
            EditorGUI.BeginChangeCheck();
            string newContent = EditorGUILayout.TextArea(rawJsonContent, textAreaStyle, GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
            {
                rawJsonContent = newContent;
                if (isFormatted)
                {
                    // 직접 편집 시 포맷팅 상태 해제
                    isFormatted = false;
                }
            }
            
            // 스크롤 끝
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.BeginHorizontal();
            
            // 저장 버튼
            EditorGUI.BeginDisabledGroup(!isEditingEnabled);
            if (GUILayout.Button("Save Changes"))
            {
                SaveSelectedData();
                EditorUtility.DisplayDialog("Save Data", "변경사항이 저장되었습니다.", "확인");
            }
            EditorGUI.EndDisabledGroup();
            
            // 포맷 토글 버튼
            string formatButtonText = isFormatted ? "Compress JSON" : "Format JSON";
            if (GUILayout.Button(formatButtonText))
            {
                if (isFormatted)
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
        string key = selectedDataType.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, "SaveData", $"{key}.json");
        
        if (File.Exists(filePath))
        {
            try
            {
                rawJsonContent = File.ReadAllText(filePath);
                unformattedJson = rawJsonContent; // 원본 JSON 저장
                Debug.Log($"{key} 데이터를 성공적으로 불러왔습니다.");
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 로드 중 오류: {e.Message}");
                rawJsonContent = string.Empty;
                EditorUtility.DisplayDialog("Error", $"데이터 로드 중 오류가 발생했습니다: {e.Message}", "확인");
            }
        }
        else
        {
            Debug.LogWarning($"선택한 데이터 파일이 존재하지 않습니다: {key}");
            rawJsonContent = string.Empty;
            EditorUtility.DisplayDialog("Warning", $"{key} 데이터 파일이 존재하지 않습니다.", "확인");
        }
    }
    
    private void SaveSelectedData()
    {
        string key = selectedDataType.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, "SaveData", $"{key}.json");
        
        try
        {
            string jsonToSave = rawJsonContent;
            
            // 포맷팅된 상태라면 압축 JSON으로 변환
            if (isFormatted)
            {
                jsonToSave = CompressJsonString(rawJsonContent);
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
            
            // 디렉토리 확인 및 생성
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            
            // 파일 저장
            File.WriteAllText(filePath, jsonToSave);
            Debug.Log($"{key} 데이터를 성공적으로 저장했습니다.");
            
            // 저장 후 상태 업데이트
            unformattedJson = jsonToSave;
            if (isFormatted)
            {
                // 포맷팅된 상태를 유지하면서 저장한 경우, 내부적으로는 압축된 JSON 저장
                rawJsonContent = FormatJsonString(jsonToSave); // 다시 포맷팅하여 표시
            }
            else
            {
                rawJsonContent = jsonToSave;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 저장 중 오류: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"데이터 저장 중 오류가 발생했습니다: {e.Message}", "확인");
        }
    }
    
    private void DeleteSelectedData()
    {
        string key = selectedDataType.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, "SaveData", $"{key}.json");
        
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"{key} 데이터를 성공적으로 삭제했습니다.");
                rawJsonContent = string.Empty;
                unformattedJson = string.Empty;
                isFormatted = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 삭제 중 오류: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"데이터 삭제 중 오류가 발생했습니다: {e.Message}", "확인");
            }
        }
        else
        {
            Debug.LogWarning($"삭제할 데이터 파일이 존재하지 않습니다: {key}");
            EditorUtility.DisplayDialog("Warning", $"{key} 데이터 파일이 존재하지 않습니다.", "확인");
        }
    }
    
    // JSON 포맷팅 함수
    private void FormatJSON()
    {
        if (string.IsNullOrEmpty(rawJsonContent))
            return;
            
        try
        {
            // 유효한 JSON인지 확인
            if (!IsValidJson(rawJsonContent))
            {
                EditorUtility.DisplayDialog("Error", "유효하지 않은 JSON 형식입니다.", "확인");
                return;
            }
            
            // 원본 JSON 저장 (압축된 형태)
            if (!isFormatted)
            {
                unformattedJson = rawJsonContent;
            }
            
            // 들여쓰기 추가
            string formattedJson = FormatJsonString(rawJsonContent);
            rawJsonContent = formattedJson;
            isFormatted = true;
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
        if (!isFormatted || string.IsNullOrEmpty(rawJsonContent))
            return;
            
        try
        {
            // 유효한 JSON인지 확인
            if (!IsValidJson(rawJsonContent))
            {
                EditorUtility.DisplayDialog("Error", "유효하지 않은 JSON 형식입니다.", "확인");
                return;
            }
            
            // 원본 (압축된) JSON으로 복원하거나 새로 압축
            rawJsonContent = !string.IsNullOrEmpty(unformattedJson) 
                ? unformattedJson 
                : CompressJsonString(rawJsonContent);
                
            isFormatted = false;
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