using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 열린 팝업들을 모니터링하는 에디터 모니터
/// </summary>
public class PopupMonitor : IEditorMonitor
{
    private const float TICK = 1f;

    private float _lastTickedTime = 0f;
    private float _nextTickTime = 0f;
    private Vector2 _scrollPosition;

    private readonly List<Popup> _openedPopups = new();

    public string MonitorName => "Popup Monitor";

    public void OnMonitorEnable()
    {
        _openedPopups.Clear();
        _lastTickedTime = 0f;
        _nextTickTime = 0f;
    }

    public void OnMonitorDisable()
    {
        _openedPopups.Clear();
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

        if (_openedPopups.Count == 0)
        {
            EditorGUILayout.HelpBox("열린 팝업이 없습니다.", MessageType.Info);
            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        {
            foreach (var popup in _openedPopups)
            {
                if (popup != null)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        // 오브젝트 아이콘과 이름을 버튼에 표시
                        var content = EditorGUIUtility.ObjectContent(popup.gameObject, typeof(GameObject));

                        // 클릭 시 Hierarchy에서 오브젝트 하이라이트
                        if (GUILayout.Button(content, EditorStyles.label, GUILayout.Height(30)))
                        {
                            EditorGUIUtility.PingObject(popup.gameObject);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void UpdateMonitor()
    {
        _openedPopups.Clear();

        var popups = Object.FindObjectsByType<Popup>(FindObjectsSortMode.None);
        if (popups == null || popups.Length == 0)
            return;

        _openedPopups.AddRange(popups);
    }
}
