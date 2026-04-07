using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 여러 에디터 모니터들을 관리하는 메인 윈도우
/// </summary>
public class EditorMonitorManager : EditorWindow
{
    private static EditorMonitorManager _instance;

    private readonly List<IEditorMonitor> _monitors = new();
    private string[] _monitorNames = Array.Empty<string>();
    private int _selectedMonitorIndex = 0;
    private int _previousMonitorIndex = -1;

    [MenuItem("CustomAnalize/MonitorManager")]
    public static void ShowWindow()
    {
        _instance = GetWindow<EditorMonitorManager>("Monitor Manager");
        _instance.minSize = new Vector2(800, 600);
        _instance.Show();
    }

    private void OnEnable()
    {
        _instance = this;
        RegisterAllMonitors();
        UpdateMonitorNames();

        // 첫 번째 모니터 활성화
        if (_monitors.Count > 0)
        {
            _monitors[0].OnMonitorEnable();
            _previousMonitorIndex = 0;
        }
    }

    private void OnDisable()
    {
        // 현재 활성화된 모니터 비활성화
        if (_previousMonitorIndex >= 0 && _previousMonitorIndex < _monitors.Count)
        {
            _monitors[_previousMonitorIndex].OnMonitorDisable();
        }
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawCurrentMonitor();
    }

    private void Update()
    {
        // 플레이 모드에서만 업데이트
        if (Application.isPlaying == false)
            return;

        if (_selectedMonitorIndex >= 0 && _selectedMonitorIndex < _monitors.Count)
        {
            _monitors[_selectedMonitorIndex].OnMonitorUpdate();
        }

        Repaint();
    }

    /// <summary>
    /// 툴바 렌더링
    /// </summary>
    private void DrawToolbar()
    {
        if (_monitorNames.Length == 0)
        {
            EditorGUILayout.HelpBox("등록된 모니터가 없습니다.", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            // 툴바 버튼으로 모니터 선택
            _selectedMonitorIndex = GUILayout.Toolbar(
                _selectedMonitorIndex,
                _monitorNames,
                EditorStyles.toolbarButton,
                GUILayout.Height(25)
            );

            // 모니터 변경 감지
            if (_selectedMonitorIndex != _previousMonitorIndex)
            {
                OnMonitorChanged(_previousMonitorIndex, _selectedMonitorIndex);
                _previousMonitorIndex = _selectedMonitorIndex;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
    }

    /// <summary>
    /// 현재 선택된 모니터 렌더링
    /// </summary>
    private void DrawCurrentMonitor()
    {
        if (_selectedMonitorIndex < 0 || _selectedMonitorIndex >= _monitors.Count)
            return;

        _monitors[_selectedMonitorIndex].OnMonitorGUI();
    }

    /// <summary>
    /// 모니터 변경 시 호출
    /// </summary>
    private void OnMonitorChanged(int previousIndex, int newIndex)
    {
        // 이전 모니터 비활성화
        if (previousIndex >= 0 && previousIndex < _monitors.Count)
        {
            _monitors[previousIndex].OnMonitorDisable();
        }

        // 새 모니터 활성화
        if (newIndex >= 0 && newIndex < _monitors.Count)
        {
            _monitors[newIndex].OnMonitorEnable();
        }
    }

    /// <summary>
    /// 모든 모니터 등록
    /// </summary>
    private void RegisterAllMonitors()
    {
        _monitors.Clear();

        // IEditorMonitor를 구현한 모든 타입 찾기
        var monitorTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IEditorMonitor).IsAssignableFrom(type)
                           && type.IsInterface == false
                           && type.IsAbstract == false);

        foreach (var type in monitorTypes)
        {
            try
            {
                var monitor = (IEditorMonitor)Activator.CreateInstance(type);
                _monitors.Add(monitor);
            }
            catch (Exception e)
            {
                Debug.LogError($"모니터 생성 실패: {type.Name}, Error: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 모니터 이름 배열 업데이트
    /// </summary>
    private void UpdateMonitorNames()
    {
        _monitorNames = _monitors.Select(m => m.MonitorName).ToArray();
    }

    /// <summary>
    /// 외부에서 모니터 등록
    /// </summary>
    public static void RegisterMonitor(IEditorMonitor monitor)
    {
        if (_instance == null)
            return;

        if (_instance._monitors.Contains(monitor) == false)
        {
            _instance._monitors.Add(monitor);
            _instance.UpdateMonitorNames();
        }
    }

    /// <summary>
    /// 외부에서 모니터 해제
    /// </summary>
    public static void UnregisterMonitor(IEditorMonitor monitor)
    {
        if (_instance == null)
            return;

        if (_instance._monitors.Remove(monitor))
        {
            _instance.UpdateMonitorNames();
        }
    }
}
