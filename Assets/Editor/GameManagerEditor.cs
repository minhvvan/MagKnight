using System;
using System.Collections;
using hvvan;
using Moon;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터를 그리기
        base.OnInspectorGUI();
        
        // 타겟 컴포넌트 참조 가져오기
        GameManager gameManager = (GameManager)target;
        
        // 여백 추가
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("상태 디버그 정보", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 상태별 색상 지정
        GUI.backgroundColor = gameManager.CurrentGameState switch
        {
            GameState.Title => new Color(0.3f, 0.3f, 0.9f, 1f),       // 파란색 계열 - 타이틀
            GameState.None => new Color(0.5f, 0.5f, 0.5f, 1f),        // 회색 - 기본 상태
            GameState.InitGame => new Color(0.8f, 0.8f, 0.2f, 1f),    // 노란색 - 초기화 중
            GameState.BaseCamp => new Color(0.2f, 0.6f, 0.3f, 1f),    // 초록색 - 안전 지역
            GameState.DungeonEnter => new Color(0.6f, 0.4f, 0.2f, 1f),// 갈색 - 던전 입장
            GameState.RoomEnter => new Color(0.5f, 0.2f, 0.7f, 1f),   // 보라색 - 방 입장
            GameState.RoomClear => new Color(0.2f, 0.8f, 0.6f, 1f),   // 청록색 - 방 클리어
            GameState.Dialogue => new Color(0.7f, 0.7f, 0.9f, 1f),    // 연한 보라색 - 대화
            GameState.GameClear => new Color(1f, 0.8f, 0.2f, 1f),     // 황금색 - 게임 클리어
            GameState.GameOver => new Color(0.8f, 0.2f, 0.2f, 1f),    // 빨간색 - 게임 오버
            GameState.Pause => new Color(0.7f, 0.7f, 0.7f, 1f),       // 밝은 회색 - 일시정지
            GameState.Max => new Color(0.1f, 0.1f, 0.1f, 1f),         // 어두운 회색 - 최대값
        };

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("현재 상태", gameManager.CurrentGameState.ToString(),
            EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndVertical();
        
        // 강제로 상태 변경 버튼
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("None"))
            gameManager.ChangeGameState(GameState.None);
        if (GUILayout.Button("Title"))
            gameManager.ChangeGameState(GameState.Title);
        if (GUILayout.Button("InitGame"))
            gameManager.ChangeGameState(GameState.InitGame);
        if (GUILayout.Button("BaseCamp"))
            gameManager.ChangeGameState(GameState.BaseCamp);
        if (GUILayout.Button("DungeonEnter"))
            gameManager.ChangeGameState(GameState.DungeonEnter);
        if (GUILayout.Button("RoomEnter"))
            gameManager.ChangeGameState(GameState.RoomEnter);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("RoomClear"))
            gameManager.ChangeGameState(GameState.RoomClear);
        if (GUILayout.Button("Dialogue"))
            gameManager.ChangeGameState(GameState.Dialogue);
        if (GUILayout.Button("GameClear"))
            gameManager.ChangeGameState(GameState.GameClear);
        if (GUILayout.Button("GameOver"))
            gameManager.ChangeGameState(GameState.GameOver);
        if (GUILayout.Button("Pause"))
            gameManager.ChangeGameState(GameState.Pause);

        EditorGUILayout.EndHorizontal();
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
