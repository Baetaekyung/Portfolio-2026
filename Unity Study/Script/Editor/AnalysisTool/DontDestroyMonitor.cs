using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// DontDestroyOnLoad 오브젝트들을 모니터링하는 에디터 모니터
/// </summary>
public class DontDestroyMonitor : IEditorMonitor
{
    private const float TICK = 1f;

    private float _lastTickedTime = 0f;
    private float _nextTickTime = 0f;
    private Vector2 _scrollPosition;

    private readonly List<DontDestroyBehaviour> _dontDestroyBehaviours = new();

    public string MonitorName => "DontDestroy Monitor";

    public void OnMonitorEnable()
    {
        _dontDestroyBehaviours.Clear();
        _lastTickedTime = 0f;
        _nextTickTime = 0f;
    }

    public void OnMonitorDisable()
    {
        _dontDestroyBehaviours.Clear();
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

        if (_dontDestroyBehaviours.Count == 0)
        {
            EditorGUILayout.HelpBox("DontDestroy 오브젝트가 없습니다.", MessageType.Info);
            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        {
            foreach (var dontDestroyObj in _dontDestroyBehaviours)
            {
                if (dontDestroyObj != null)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        // 오브젝트 아이콘과 이름을 버튼에 표시
                        var content = EditorGUIUtility.ObjectContent(dontDestroyObj.gameObject, typeof(GameObject));

                        // 클릭 시 Hierarchy에서 오브젝트 하이라이트
                        if (GUILayout.Button(content, EditorStyles.label, GUILayout.Height(30)))
                        {
                            EditorGUIUtility.PingObject(dontDestroyObj.gameObject);
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
        _dontDestroyBehaviours.Clear();

        var behaviours = Object.FindObjectsByType<DontDestroyBehaviour>(FindObjectsSortMode.None);
        if (behaviours == null || behaviours.Length == 0)
            return;

        _dontDestroyBehaviours.AddRange(behaviours);
    }
}
