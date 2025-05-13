using System;
using hvvan;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomController))]
public class RoomControllerEditor : Editor
{
    private bool showRoomInfo = true;
    private bool showGatesInfo = true;
    private bool showClearStatus = true;
    private GUIStyle headerStyle;
    private GUIStyle subHeaderStyle;
    private GUIStyle statusStyle;

    public override void OnInspectorGUI()
    {
        // 스타일 초기화
        InitStyles();
        
        // 기본 인스펙터를 그리기
        base.OnInspectorGUI();
        
        // 타겟 컴포넌트 참조 가져오기
        RoomController roomController = (RoomController)target;
        
        if (roomController.Room == null)
        {
            EditorGUILayout.HelpBox("Room 데이터가 설정되지 않았습니다.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Room 디버그 정보", headerStyle);

        // Room 정보 표시
        DrawRoomInfo(roomController);

        // Gates 정보 표시
        DrawGatesInfo(roomController);

        // 클리어 상태 표시
        DrawClearStatus(roomController);
    }

    private void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.margin = new RectOffset(0, 0, 10, 5);
        }

        if (subHeaderStyle == null)
        {
            subHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
            subHeaderStyle.fontSize = 12;
            subHeaderStyle.margin = new RectOffset(0, 0, 5, 5);
        }

        if (statusStyle == null)
        {
            statusStyle = new GUIStyle(EditorStyles.label);
            statusStyle.fontStyle = FontStyle.Bold;
        }
    }
    
    // Room 타입 문자열 반환 메서드
    private string GetRoomTypeString(Room room)
    {
        if (room == null)
            return "알 수 없음";
            
        // Room 클래스의 roomType 필드 사용
        switch (room.roomType)
        {
            case RoomType.None:
                return "없음";
            case RoomType.StartRoom:
                return "시작 방";
            case RoomType.BossRoom:
                return "보스 방";
            case RoomType.ShopRoom:
                return "상점 방";
            case RoomType.BattleRoom:
                return "전투 방";
            case RoomType.TreasureRoom:
                return "보물 방";
            case RoomType.TrapRoom:
                return "함정 방";
            default:
                return "알 수 없음";
        }
    }

    private void DrawRoomInfo(RoomController roomController)
    {
        showRoomInfo = EditorGUILayout.Foldout(showRoomInfo, "Room 정보", true, EditorStyles.foldoutHeader);
        
        if (showRoomInfo)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("Room 인덱스:", roomController.RoomIndex.ToString());
            
            if (roomController.Room != null)
            {
                // Room 타입 표시
                EditorGUILayout.LabelField("Room 타입:", GetRoomTypeString(roomController.Room));
                
                EditorGUILayout.LabelField("제목:", roomController.Room.roomTitle);

                // 씬 이름 표시
                EditorGUILayout.LabelField("씬 이름:", roomController.Room.sceneName);
                
                EditorGUILayout.LabelField("연결된 방:");
                EditorGUI.indentLevel++;
                
                string[] directions = { "동쪽", "남쪽", "서쪽", "북쪽" };
                
                for (int i = 0; i < 4; i++)
                {
                    string status = roomController.Room.connectedRooms[i] == Room.Empty ? "없음" : 
                                   roomController.Room.connectedRooms[i] == Room.Blocked ? "막힘" : 
                                   roomController.Room.connectedRooms[i].ToString();
                    
                    // 연결된 방의 타입 정보 가져오기
                    string roomType = "알 수 없음";
                    int connectedRoomIndex = roomController.Room.connectedRooms[i];
                    if (connectedRoomIndex >= 0)
                    {
                        // RoomGenerator를 통해 방 데이터 접근
                        var roomSceneController = FindObjectOfType<RoomSceneController>();
                        if (roomSceneController != null)
                        {
                            var roomData = GetRoomByIndex(roomSceneController, connectedRoomIndex);
                            if (roomData != null)
                            {
                                roomType = GetRoomTypeString(roomData);
                            }
                        }
                    }
                    
                    EditorGUILayout.LabelField($"{directions[i]}: {status} ({roomType})");
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }
    
    // RoomSceneController에서 방 데이터 가져오기
    private Room GetRoomByIndex(RoomSceneController controller, int index)
    {
        // RoomGenerator의 GetRoom 메서드 호출을 위한 리플렉션 사용
        var generatorField = typeof(RoomSceneController).GetField("_roomGenerator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (generatorField != null)
        {
            var generator = generatorField.GetValue(controller);
            if (generator != null)
            {
                var getRoom = generator.GetType().GetMethod("GetRoom");
                if (getRoom != null)
                {
                    return getRoom.Invoke(generator, new object[] { index }) as Room;
                }
            }
        }
        return null;
    }

    private void DrawGatesInfo(RoomController roomController)
    {
        showGatesInfo = EditorGUILayout.Foldout(showGatesInfo, "Gates 정보", true, EditorStyles.foldoutHeader);
        
        if (showGatesInfo)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUI.indentLevel++;
            
            string[] directions = { "East", "North", "West", "South" };
            
            foreach (string dir in directions)
            {
                RoomDirection direction = (RoomDirection)Enum.Parse(typeof(RoomDirection), dir);
                bool hasGate = roomController.Room != null && 
                              roomController.Room.connectedRooms[(int)direction] != Room.Empty && 
                              roomController.Room.connectedRooms[(int)direction] != Room.Blocked;
                
                if (hasGate)
                {
                    bool isActive = false;
                    // Gate 활성화 상태 확인 (reflection으로 gates Dictionary에 접근하기)
                    var gatesField = typeof(RoomController).GetField("gates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (gatesField != null)
                    {
                        var gates = gatesField.GetValue(roomController);
                        var containsMethod = gates.GetType().GetMethod("ContainsKey");
                        var getItemMethod = gates.GetType().GetMethod("get_Item");
                        
                        if (containsMethod != null && getItemMethod != null)
                        {
                            bool containsKey = (bool)containsMethod.Invoke(gates, new object[] { direction });
                            if (containsKey)
                            {
                                var gate = getItemMethod.Invoke(gates, new object[] { direction });
                                if (gate != null)
                                {
                                    isActive = ((Component)gate).gameObject.activeSelf;
                                }
                            }
                        }
                    }
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{dir} Gate:");
                    
                    GUIStyle gateStyle = new GUIStyle(EditorStyles.label);
                    gateStyle.normal.textColor = isActive ? Color.green : Color.red;
                    EditorGUILayout.LabelField(isActive ? "활성화" : "비활성화", gateStyle);
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawClearStatus(RoomController roomController)
    {
        showClearStatus = EditorGUILayout.Foldout(showClearStatus, "클리어 상태", true, EditorStyles.foldoutHeader);
        
        if (showClearStatus)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUI.indentLevel++;
            
            // CurrentRunData에서 클리어 상태를 가져오기
            bool isCleared = false;
            string clearStatus = "아직 클리어되지 않음";
            
            // GameManager를 통해 CurrentRunData 접근하기
            var currentRunData = GameManager.Instance.CurrentRunData;
            if (currentRunData != null)
            {
                isCleared = currentRunData.clearedRooms.Contains(roomController.RoomIndex);
                clearStatus = isCleared ? "클리어됨" : "아직 클리어되지 않음";
                
                // 현재 층 표시
                EditorGUILayout.LabelField("현재 층:", currentRunData.currentFloor.ToString());
            }
            
            GUIStyle clearStyle = new GUIStyle(statusStyle);
            clearStyle.normal.textColor = isCleared ? Color.green : Color.yellow;
            
            EditorGUILayout.LabelField("클리어 상태:", clearStatus, clearStyle);
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }

    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (target != null)
            Repaint();
    }
}