using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using AYellowpaper.SerializedCollections;

public class ArtifactManagerWindow : EditorWindow
{
    private List<ArtifactDataSO> artifacts = new List<ArtifactDataSO>();
    private Vector2 scrollPosition;
    private bool isDirty = false;
    private Dictionary<int, List<ArtifactDataSO>> duplicateIDs = new Dictionary<int, List<ArtifactDataSO>>();
    private string searchFilter = "";
    private bool showDuplicatesOnly = false;
    private bool isEditingID = false;
    private ArtifactDataSO currentEditingArtifact = null;
    private int highestID = 0;
    
    // 매핑 SO 관련
    private ArtifactDataMappingSO currentMappingSO = null;
    private string mappingSOPath = "Assets/Data/Artifact/ArtifactDataMappingSO.asset";
    private bool hasDuplicateIDs = false;

    // 정렬 옵션
    private enum SortOption
    {
        ID,
        Name,
        ScrapValue
    }
    private SortOption currentSortOption = SortOption.ID;
    private bool sortAscending = true;

    // GUI 스타일들
    private GUIStyle headerStyle;
    private GUIStyle itemStyle;
    private GUIStyle duplicateStyle;
    private GUIStyle selectedStyle;
    private GUIStyle wrapTextStyle;
    private GUIStyle warningBoxStyle;
    private bool stylesInitialized = false;
    
    // 희귀도별 색상 정의
    private Dictionary<ItemRarity, Color> rarityColors = new Dictionary<ItemRarity, Color>()
    {
        { ItemRarity.Common, Color.white },
        { ItemRarity.Uncommon, new Color(0.2f, 1f, 0.2f) },         
        { ItemRarity.Rare, new Color(0.4f, 0.7f, 1f) },
        { ItemRarity.Epic, new Color(0.9f, 0.4f, 1f) },
        { ItemRarity.Legendary, new Color(1f, 0.9f, 0.2f) }
    };

    [MenuItem("Tools/ArtifactManager")]
    public static void ShowWindow()
    {
        var window = GetWindow<ArtifactManagerWindow>("ArtifactManager");
        window.minSize = new Vector2(800, 500);
        window.Show();
    }

    private void OnEnable()
    {
        FindAllArtifacts();
        CheckForDuplicateIDs();
        FindMappingSO();
        // 스타일은 OnGUI에서 초기화
    }

    private void FindMappingSO()
    {
        // 기존 매핑 SO 찾기
        currentMappingSO = AssetDatabase.LoadAssetAtPath<ArtifactDataMappingSO>(mappingSOPath);
        
        // 경로에 없다면 프로젝트 내 검색
        if (currentMappingSO == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:ArtifactDataMappingSO");
            if (guids.Length > 0)
            {
                string foundPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                currentMappingSO = AssetDatabase.LoadAssetAtPath<ArtifactDataMappingSO>(foundPath);
                mappingSOPath = foundPath;
            }
        }
    }

    private void InitializeStyles()
    {
        if (stylesInitialized || EditorStyles.label == null)
            return;

        headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.alignment = TextAnchor.MiddleLeft;
        headerStyle.fontSize = 12;

        itemStyle = new GUIStyle(EditorStyles.label);
        itemStyle.padding = new RectOffset(5, 5, 2, 2);
        itemStyle.margin = new RectOffset(0, 0, 1, 1);

        duplicateStyle = new GUIStyle(itemStyle);
        duplicateStyle.normal.background = MakeColorTexture(new Color(1f, 0.5f, 0.5f, 0.3f));

        selectedStyle = new GUIStyle(itemStyle);
        selectedStyle.normal.background = MakeColorTexture(new Color(0.3f, 0.6f, 1f, 0.3f));

        wrapTextStyle = new GUIStyle(EditorStyles.textField);
        wrapTextStyle.wordWrap = true;

        warningBoxStyle = new GUIStyle(EditorStyles.helpBox);
        warningBoxStyle.normal.textColor = Color.yellow;
        warningBoxStyle.fontSize = 12;
        warningBoxStyle.alignment = TextAnchor.MiddleCenter;

        stylesInitialized = true;
    }

    private Texture2D MakeColorTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    private void OnGUI()
    {
        InitializeStyles();

        EditorGUILayout.Space(10);
        DrawToolbar();
        EditorGUILayout.Space(5);

        // 중복 ID 경고 표시
        if (hasDuplicateIDs)
        {
            EditorGUILayout.BeginHorizontal(warningBoxStyle);
            EditorGUILayout.LabelField("경고: 중복된 ID가 있습니다! 매핑 SO를 생성하기 전에 ID 충돌을 해결하세요.", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        // 매핑 SO 정보 표시
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        if (currentMappingSO != null)
        {
            EditorGUILayout.LabelField($"매핑 SO: {AssetDatabase.GetAssetPath(currentMappingSO)} ({currentMappingSO.artifacts.Count}개 항목)");
        }
        else
        {
            EditorGUILayout.LabelField("매핑 SO가 없습니다.");
        }
        
        if (GUILayout.Button("매핑 SO 생성/업데이트", GUILayout.Width(150)))
        {
            CreateOrUpdateMappingSO();
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);

        // 검색 및 필터 옵션
        EditorGUILayout.BeginHorizontal();
        searchFilter = EditorGUILayout.TextField("검색:", searchFilter, GUILayout.Width(300));
        if (GUILayout.Button("초기화", GUILayout.Width(60)))
        {
            searchFilter = "";
            GUI.FocusControl(null);
        }
        GUILayout.FlexibleSpace();
        showDuplicatesOnly = EditorGUILayout.ToggleLeft("중복 ID만 표시", showDuplicatesOnly, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        // 정렬 옵션
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("정렬:", GUILayout.Width(50));
        SortOption newSortOption = (SortOption)EditorGUILayout.EnumPopup(currentSortOption, GUILayout.Width(100));
        if (newSortOption != currentSortOption)
        {
            currentSortOption = newSortOption;
            SortArtifacts();
        }

        bool newSortDirection = EditorGUILayout.ToggleLeft(sortAscending ? "오름차순" : "내림차순", sortAscending, GUILayout.Width(100));
        if (newSortDirection != sortAscending)
        {
            sortAscending = newSortDirection;
            SortArtifacts();
        }
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("ID 자동 할당", GUILayout.Width(120)))
        {
            if (EditorUtility.DisplayDialog("ID 자동 할당", 
                "모든 아티팩트 ID를 0부터 순차적으로 다시 할당합니다. 이 작업은 취소할 수 없습니다. 계속하시겠습니까?", 
                "예", "아니오"))
            {
                AutoAssignIDs();
            }
        }
        
        if (GUILayout.Button("목록 새로고침", GUILayout.Width(100)))
        {
            FindAllArtifacts();
            CheckForDuplicateIDs();
        }
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // 헤더
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("ID", headerStyle, GUILayout.Width(60));
        EditorGUILayout.LabelField("", GUILayout.Width(60)); // 아이콘 영역
        EditorGUILayout.LabelField("이름", headerStyle, GUILayout.Width(200));
        EditorGUILayout.LabelField("스크랩 가치", headerStyle, GUILayout.Width(100));
        EditorGUILayout.LabelField("희귀도",  headerStyle, GUILayout.Width(100));
        EditorGUILayout.LabelField("에셋 경로", headerStyle, GUILayout.Width(200));
        EditorGUILayout.LabelField("작업", headerStyle, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        // 아티팩트 리스트
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 필터링 및 정렬된 리스트 가져오기
        var filteredArtifacts = FilterArtifacts();

        foreach (var artifact in filteredArtifacts)
        {
            if (artifact == null) continue;

            bool isDuplicate = IsDuplicate(artifact);
            GUIStyle rowStyle = isDuplicate ? duplicateStyle : 
                                (artifact == currentEditingArtifact ? selectedStyle : itemStyle);

            EditorGUILayout.BeginHorizontal(rowStyle);

            // ID 필드
            EditorGUI.BeginChangeCheck();
            bool isThisEditing = (artifact == currentEditingArtifact && isEditingID);
            
            if (isThisEditing)
            {
                int newID = EditorGUILayout.IntField(artifact.itemID, GUILayout.Width(60));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(artifact, "Change Artifact ID");
                    artifact.itemID = newID;
                    EditorUtility.SetDirty(artifact);
                    isDirty = true;
                }
            }
            else
            {
                EditorGUILayout.LabelField(artifact.itemID.ToString(), GUILayout.Width(60));
            }

            // 아이콘
            EditorGUILayout.BeginHorizontal(GUILayout.Width(60));
            if (artifact.icon != null)
            {
                GUILayout.Label(AssetPreview.GetAssetPreview(artifact.icon), GUILayout.Width(50), GUILayout.Height(50));
            }
            else
            {
                GUILayout.Label("없음", GUILayout.Width(50), GUILayout.Height(50));
            }
            EditorGUILayout.EndHorizontal();

            // 이름
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.DelayedTextField(artifact.itemName, GUILayout.Width(200));
            if (EditorGUI.EndChangeCheck() && newName != artifact.itemName)
            {
                Undo.RecordObject(artifact, "Change Artifact Name");
                artifact.itemName = newName;
                EditorUtility.SetDirty(artifact);
                isDirty = true;
            }

            // 스크랩 가치
            EditorGUI.BeginChangeCheck();
            int newScrapValue = EditorGUILayout.IntField(artifact.scrapValue, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck() && newScrapValue != artifact.scrapValue)
            {
                Undo.RecordObject(artifact, "Change Artifact Scrap Value");
                artifact.scrapValue = newScrapValue;
                EditorUtility.SetDirty(artifact);
                isDirty = true;
            }

            // 아이템 희귀도
            Rect rarityRect = EditorGUILayout.GetControlRect(GUILayout.Width(100));
            DrawColoredRarityPopup(rarityRect, artifact, artifact.rarity);
            
            // 애셋 경로
            string assetPath = AssetDatabase.GetAssetPath(artifact);
            EditorGUILayout.LabelField(assetPath, GUILayout.Width(200));

            // 액션 버튼들
            EditorGUILayout.BeginHorizontal(GUILayout.Width(150));
            
            if (isThisEditing)
            {
                if (GUILayout.Button("완료", GUILayout.Width(60)))
                {
                    isEditingID = false;
                    currentEditingArtifact = null;
                    CheckForDuplicateIDs();
                    SortArtifacts();
                }
            }
            else
            {
                if (GUILayout.Button("ID 편집", GUILayout.Width(60)))
                {
                    isEditingID = true;
                    currentEditingArtifact = artifact;
                }
            }
            
            if (GUILayout.Button("선택", GUILayout.Width(60)))
            {
                Selection.activeObject = artifact;
                EditorGUIUtility.PingObject(artifact);
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // 상태 표시줄
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        string statusMessage = $"전체 아티팩트: {artifacts.Count}개";
        if (duplicateIDs.Count > 0)
        {
            statusMessage += $" | 중복 ID: {duplicateIDs.Count}개";
        }
        EditorGUILayout.LabelField(statusMessage, EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        // 변경사항이 있으면 저장
        if (isDirty && !isEditingID)
        {
            CheckForDuplicateIDs();
            isDirty = false;
        }
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("아티팩트 관리자", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("새 아티팩트 생성", EditorStyles.toolbarButton))
        {
            CreateNewArtifact();
        }
        EditorGUILayout.EndHorizontal();
    }
    
    // 매핑 SO 생성 또는 업데이트 함수
    private void CreateOrUpdateMappingSO()
    {
        // 중복 ID 검사
        if (duplicateIDs.Count > 0)
        {
            if (!EditorUtility.DisplayDialog("중복 ID 경고", 
                "중복된 ID가 있습니다. 이로 인해 매핑에 문제가 발생할 수 있습니다. 계속하시겠습니까?", 
                "계속", "취소"))
            {
                return;
            }
        }

        bool createNew = false;
        
        // 매핑 SO가 없으면 새로 생성
        if (currentMappingSO == null)
        {
            // 디렉토리 경로 확인 및 생성
            string directory = System.IO.Path.GetDirectoryName(mappingSOPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            // 새 매핑 SO 생성
            currentMappingSO = CreateInstance<ArtifactDataMappingSO>();
            createNew = true;
        }
        
        // 매핑 데이터 초기화 및 채우기
        currentMappingSO.artifacts.Clear();
        
        foreach (var artifact in artifacts)
        {
            if (artifact != null)
            {
                // 중복 ID가 있는 경우 이미 추가된 ID인지 검사
                if (!currentMappingSO.artifacts.ContainsKey(artifact.itemID))
                {
                    currentMappingSO.artifacts.Add(artifact.itemID, artifact);
                }
                else
                {
                    Debug.LogWarning($"중복 ID {artifact.itemID}는 매핑 SO에 추가되지 않았습니다: {artifact.itemName}");
                }
            }
        }
        
        // 에셋 저장
        if (createNew)
        {
            AssetDatabase.CreateAsset(currentMappingSO, mappingSOPath);
            Debug.Log($"새 매핑 SO가 생성되었습니다: {mappingSOPath}");
        }
        else
        {
            EditorUtility.SetDirty(currentMappingSO);
            Debug.Log($"매핑 SO가 업데이트되었습니다: {mappingSOPath}");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 생성된 에셋 선택
        Selection.activeObject = currentMappingSO;
        EditorGUIUtility.PingObject(currentMappingSO);
    }
    
    // 희귀도 표시를 위한 커스텀 메서드 추가
    private void DrawColoredRarityPopup(Rect position, ArtifactDataSO artifact, ItemRarity currentRarity)
    {
        // 원래 GUI 색상 저장
        Color originalColor = GUI.color;
        
        // 희귀도에 따라 GUI 색상 변경
        if (rarityColors.ContainsKey(currentRarity))
        {
            GUI.color = rarityColors[currentRarity];
        }
        
        // 팝업 표시
        EditorGUI.BeginChangeCheck();
        ItemRarity newRarity = (ItemRarity)EditorGUI.EnumPopup(position, currentRarity);
        
        // GUI 색상 복원
        GUI.color = originalColor;
        
        // 값이 변경되었는지 확인
        if (EditorGUI.EndChangeCheck() && newRarity != currentRarity)
        {
            Undo.RecordObject(artifact, "Change Artifact Rarity");
            artifact.rarity = newRarity;
            EditorUtility.SetDirty(artifact);
            isDirty = true;
        }
    }

    private void FindAllArtifacts()
    {
        artifacts.Clear();
        highestID = 0;
        
        string[] guids = AssetDatabase.FindAssets("t:ArtifactDataSO");
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ArtifactDataSO artifact = AssetDatabase.LoadAssetAtPath<ArtifactDataSO>(assetPath);
            
            if (artifact != null)
            {
                artifacts.Add(artifact);
                
                // 최고 ID 기록
                if (artifact.itemID > highestID)
                {
                    highestID = artifact.itemID;
                }
            }
        }

        SortArtifacts();
    }

    private void SortArtifacts()
    {
        switch (currentSortOption)
        {
            case SortOption.ID:
                artifacts = sortAscending ? 
                    artifacts.OrderBy(a => a.itemID).ToList() : 
                    artifacts.OrderByDescending(a => a.itemID).ToList();
                break;
                
            case SortOption.Name:
                artifacts = sortAscending ? 
                    artifacts.OrderBy(a => a.itemName).ToList() : 
                    artifacts.OrderByDescending(a => a.itemName).ToList();
                break;
                
            case SortOption.ScrapValue:
                artifacts = sortAscending ? 
                    artifacts.OrderBy(a => a.scrapValue).ToList() : 
                    artifacts.OrderByDescending(a => a.scrapValue).ToList();
                break;
        }
        
        Repaint();
    }

    private List<ArtifactDataSO> FilterArtifacts()
    {
        IEnumerable<ArtifactDataSO> filtered = artifacts;
        
        // 검색어 필터
        if (!string.IsNullOrEmpty(searchFilter))
        {
            string filter = searchFilter.ToLower();
            filtered = filtered.Where(a => 
                (a.itemName != null && a.itemName.ToLower().Contains(filter)) || 
                a.itemID.ToString().Contains(filter));
        }
        
        // 중복 ID만 표시 필터
        if (showDuplicatesOnly)
        {
            filtered = filtered.Where(a => IsDuplicate(a));
        }
        
        return filtered.ToList();
    }

    private void CheckForDuplicateIDs()
    {
        duplicateIDs.Clear();
        
        var idGroups = artifacts
            .Where(a => a != null)
            .GroupBy(a => a.itemID)
            .Where(g => g.Count() > 1);
            
        foreach (var group in idGroups)
        {
            duplicateIDs[group.Key] = group.ToList();
        }
        
        hasDuplicateIDs = duplicateIDs.Count > 0;
        Repaint();
    }

    private bool IsDuplicate(ArtifactDataSO artifact)
    {
        return duplicateIDs.ContainsKey(artifact.itemID);
    }

    private void AutoAssignIDs()
    {
        // ID로 먼저 정렬
        var sortedArtifacts = artifacts.OrderBy(a => a.itemID).ToList();
        
        for (int i = 0; i < sortedArtifacts.Count; i++)
        {
            Undo.RecordObject(sortedArtifacts[i], "Auto Assign ID");
            sortedArtifacts[i].itemID = i; // 0부터 시작하는 ID 부여
            EditorUtility.SetDirty(sortedArtifacts[i]);
        }
        
        AssetDatabase.SaveAssets();
        CheckForDuplicateIDs();
        FindAllArtifacts(); // 다시 불러와서 정렬
        
        Debug.Log($"아티팩트 {sortedArtifacts.Count}개에 ID를 자동 할당했습니다.");
    }

    private void CreateNewArtifact()
    {
        // ScriptableObject 선택 창 표시
        SOTypeSelectionWindow.ShowWindow((type) => {
            if (type != null)
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "새 아티팩트 생성",
                    "NewArtifact.asset",
                    "asset",
                    "새 아티팩트의 파일 이름을 입력하세요"
                );

                if (!string.IsNullOrEmpty(path))
                {
                    ArtifactDataSO newArtifact = (ArtifactDataSO)CreateInstance(type);
                    
                    // 자동으로 가장 높은 ID + 1이 아닌 현재 사용 중이지 않은 0부터 순차적인 ID 할당
                    int newID = 0;
                    HashSet<int> usedIDs = new HashSet<int>(artifacts.Select(a => a.itemID));
                    
                    // 0부터 시작해서 사용 중이지 않은 가장 작은 ID 찾기
                    while (usedIDs.Contains(newID))
                    {
                        newID++;
                    }
                    
                    newArtifact.itemID = newID;
                    newArtifact.itemName = $"새 {type.Name}";
                    
                    AssetDatabase.CreateAsset(newArtifact, path);
                    AssetDatabase.SaveAssets();
                    
                    FindAllArtifacts();
                    Selection.activeObject = newArtifact;
                    EditorGUIUtility.PingObject(newArtifact);
                }
            }
        });
    }
}

// ArtifactDataSO의 파생 타입을 선택하기 위한 추가 윈도우
public class SOTypeSelectionWindow : EditorWindow
{
    private List<Type> soTypes = new List<Type>();
    private Action<Type> onTypeSelected;
    private Vector2 scrollPosition;

    public static void ShowWindow(Action<Type> callback)
    {
        SOTypeSelectionWindow window = GetWindow<SOTypeSelectionWindow>("아티팩트 타입 선택");
        window.minSize = new Vector2(300, 400);
        window.onTypeSelected = callback;
        window.FindSOTypes();
        window.ShowModal();
    }

    private void FindSOTypes()
    {
        soTypes.Clear();
        
        // 모든 ArtifactDataSO 파생 타입 찾기
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(ArtifactDataSO)) && !type.IsAbstract)
                    {
                        soTypes.Add(type);
                    }
                }
            }
            catch (Exception) { /* Some assemblies might throw exceptions when getting types */ }
        }
        
        // 이름으로 정렬
        soTypes = soTypes.OrderBy(t => t.Name).ToList();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("아티팩트 타입 선택", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (var type in soTypes)
        {
            if (GUILayout.Button(type.Name, GUILayout.Height(30)))
            {
                onTypeSelected?.Invoke(type);
                Close();
            }
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("취소"))
        {
            Close();
        }
    }
}
#endif