using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 실행 중인 TweenBehaviour들을 모니터링하는 에디터 모니터
/// </summary>
public class ActiveTweenMonitor : IEditorMonitor
{
    private const float TICK = 0.5f;

    private float _lastTickedTime = 0f;
    private float _nextTickTime = 0f;
    private Vector2 _scrollPosition;
    private bool _showOnlyPlaying = false;

    private readonly List<TweenBehaviour> _tweenBehaviours = new();

    public string MonitorName => "Active Tween Monitor";

    public void OnMonitorEnable()
    {
        _tweenBehaviours.Clear();
        _lastTickedTime = 0f;
        _nextTickTime = 0f;
    }

    public void OnMonitorDisable()
    {
        _tweenBehaviours.Clear();
    }

    public void OnMonitorUpdate()
    {
        // 타이머 기반 데이터 업데이트
        if (_lastTickedTime >= _nextTickTime)
        {
            _nextTickTime += TICK;
            UpdateMonitor();
        }

        _lastTickedTime += Time.unscaledDeltaTime;
    }

    public void OnMonitorGUI()
    {
        if (Application.isPlaying == false)
        {
            EditorGUILayout.HelpBox("플레이 모드에서만 사용 가능합니다.", MessageType.Info);
            return;
        }

        DrawHeader();

        if (_tweenBehaviours.Count == 0)
        {
            EditorGUILayout.HelpBox("TweenBehaviour가 없습니다.", MessageType.Info);
            return;
        }

        DrawTweenList();
    }

    // 헤더 UI 그리기
    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal();
        {
            // 재생 중인 것만 표시 토글
            _showOnlyPlaying = EditorGUILayout.ToggleLeft("재생 중인 Tween만 표시", _showOnlyPlaying, GUILayout.Width(200));

            // 통계 표시
            var playingCount = 0;
            foreach (var tween in _tweenBehaviours)
            {
                if (tween != null && tween.IsPlaying)
                    playingCount++;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"전체: {_tweenBehaviours.Count} | 재생 중: {playingCount}", EditorStyles.boldLabel);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
    }

    // Tween 목록 그리기
    private void DrawTweenList()
    {
        // 헤더 행
        EditorGUILayout.BeginHorizontal("box");
        {
            GUILayout.Label("오브젝트", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.Label("타입", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.Label("상태", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Duration", EditorStyles.boldLabel, GUILayout.Width(70));
            GUILayout.Label("Strength", EditorStyles.boldLabel, GUILayout.Width(70));
            GUILayout.Label("Loop", EditorStyles.boldLabel, GUILayout.Width(50));
            GUILayout.Label("제어", EditorStyles.boldLabel, GUILayout.Width(100));
        }
        EditorGUILayout.EndHorizontal();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        {
            foreach (var tween in _tweenBehaviours)
            {
                if (tween == null)
                    continue;

                // 재생 중인 것만 표시 옵션
                if (_showOnlyPlaying && tween.IsPlaying == false)
                    continue;

                DrawTweenRow(tween);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    // 개별 Tween 행 그리기
    private void DrawTweenRow(TweenBehaviour tween)
    {
        var isPlaying = tween.IsPlaying;
        var backgroundColor = GUI.backgroundColor;

        // 재생 중인 항목 강조
        if (isPlaying)
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 0.5f);

        EditorGUILayout.BeginHorizontal("box");
        {
            // 오브젝트 선택 버튼
            var content = EditorGUIUtility.ObjectContent(tween.gameObject, typeof(GameObject));
            if (GUILayout.Button(content, EditorStyles.label, GUILayout.Width(200), GUILayout.Height(20)))
            {
                EditorGUIUtility.PingObject(tween.gameObject);
                Selection.activeGameObject = tween.gameObject;
            }

            // 애니메이션 타입
            GUILayout.Label(tween.AnimationType.ToString(), GUILayout.Width(100));

            // 상태
            var statusStyle = new GUIStyle(EditorStyles.label);
            if (isPlaying)
            {
                statusStyle.normal.textColor = Color.green;
                GUILayout.Label("재생 중", statusStyle, GUILayout.Width(80));
            }
            else
            {
                statusStyle.normal.textColor = Color.gray;
                GUILayout.Label("정지", statusStyle, GUILayout.Width(80));
            }

            // Duration
            GUILayout.Label(tween.Duration.ToString("F2"), GUILayout.Width(70));

            // Strength
            GUILayout.Label(tween.Strength.ToString("F2"), GUILayout.Width(70));

            // Loop
            GUILayout.Label(tween.IsLoop ? "O" : "X", GUILayout.Width(50));

            // 제어 버튼
            if (isPlaying)
            {
                if (GUILayout.Button("정지", GUILayout.Width(50)))
                {
                    tween.Stop();
                }
            }
            else
            {
                if (GUILayout.Button("재생", GUILayout.Width(50)))
                {
                    tween.Play();
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = backgroundColor;
    }

    private void UpdateMonitor()
    {
        _tweenBehaviours.Clear();

        var behaviours = Object.FindObjectsByType<TweenBehaviour>(FindObjectsSortMode.None);
        if (behaviours == null || behaviours.Length == 0)
            return;

        _tweenBehaviours.AddRange(behaviours);
    }
}
