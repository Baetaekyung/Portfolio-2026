using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/*
에디터 메뉴에 씬 선택 기능 추가
메뉴: Scenes/[씬 이름]
단축키: Ctrl+Shift+S로 씬 선택 팝업 표시
*/
public static class SceneToolbar
{
    // 단축키로 씬 선택 팝업 열기
    [MenuItem("Scenes/Select Scene... %#s", priority = 0)]
    private static void OpenSceneSelector()
    {
        SceneSelectorPopup.Show();
    }

    [MenuItem("Scenes/", priority = 10)]
    private static void Separator() { }

    // Build Settings 열기
    [MenuItem("Scenes/Open Build Settings", priority = 1000)]
    private static void OpenBuildSettings()
    {
        EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
    }
}

// 씬 선택 팝업 윈도우
public class SceneSelectorPopup : EditorWindow
{
    private static string[] _scenePaths;
    private static string[] _sceneNames;
    private string _searchText = "";
    private Vector2 _scrollPos;
    private int _selectedIndex = -1;

    public new static void Show()
    {
        RefreshSceneList();

        // 이미 열린 윈도우가 있으면 포커스
        var window = GetWindow<SceneSelectorPopup>(false, "씬 선택", true);
        window.minSize = new Vector2(200, 150);
        window.Focus();
    }

    private static void RefreshSceneList()
    {
        var scenes = EditorBuildSettings.scenes;
        _scenePaths = scenes.Select(s => s.path).ToArray();
        _sceneNames = scenes.Select(s => Path.GetFileNameWithoutExtension(s.path)).ToArray();
    }

    private void OnEnable()
    {
        if (_scenePaths == null)
        {
            RefreshSceneList();
        }

        // 현재 씬 인덱스 찾기
        var currentPath = EditorSceneManager.GetActiveScene().path;
        for (int i = 0; i < _scenePaths.Length; i++)
        {
            if (_scenePaths[i] == currentPath)
            {
                _selectedIndex = i;
                break;
            }
        }
    }

    private void OnGUI()
    {
        // 검색 필드
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUI.SetNextControlName("SearchField");
        _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);
        if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
        {
            _searchText = "";
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();

        // 첫 프레임에 검색 필드에 포커스
        if (Event.current.type == EventType.Repaint && string.IsNullOrEmpty(_searchText))
        {
            EditorGUI.FocusTextInControl("SearchField");
        }

        // 키보드 입력 처리
        HandleKeyboardInput();

        // 씬 목록
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        for (int i = 0; i < _sceneNames.Length; i++)
        {
            // 검색 필터
            if (!string.IsNullOrEmpty(_searchText) &&
                !_sceneNames[i].ToLower().Contains(_searchText.ToLower()))
            {
                continue;
            }

            var isCurrentScene = EditorSceneManager.GetActiveScene().path == _scenePaths[i];
            var isSelected = i == _selectedIndex;

            // 스타일 설정
            var style = new GUIStyle(EditorStyles.label);
            if (isCurrentScene)
            {
                style.fontStyle = FontStyle.Bold;
            }

            var rect = EditorGUILayout.BeginHorizontal();

            // 선택 배경
            if (isSelected)
            {
                EditorGUI.DrawRect(rect, new Color(0.24f, 0.48f, 0.90f, 0.5f));
            }

            // 체크 표시
            if (isCurrentScene)
            {
                GUILayout.Label("✓", GUILayout.Width(20));
            }
            else
            {
                GUILayout.Space(24);
            }

            // 씬 이름 버튼
            if (GUILayout.Button(_sceneNames[i], style))
            {
                OpenScene(i);
            }

            EditorGUILayout.EndHorizontal();

            // 더블클릭 또는 마우스 클릭으로 선택
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                _selectedIndex = i;
                if (Event.current.clickCount == 2)
                {
                    OpenScene(i);
                }
                Repaint();
            }
        }

        EditorGUILayout.EndScrollView();

        // 하단 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("새로고침"))
        {
            RefreshSceneList();
        }
        if (GUILayout.Button("Build Settings"))
        {
            EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            Close();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void HandleKeyboardInput()
    {
        var e = Event.current;
        if (e.type != EventType.KeyDown) return;

        switch (e.keyCode)
        {
            case KeyCode.UpArrow:
                _selectedIndex = Mathf.Max(0, _selectedIndex - 1);
                e.Use();
                Repaint();
                break;

            case KeyCode.DownArrow:
                _selectedIndex = Mathf.Min(_sceneNames.Length - 1, _selectedIndex + 1);
                e.Use();
                Repaint();
                break;

            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                if (_selectedIndex >= 0)
                {
                    OpenScene(_selectedIndex);
                }
                e.Use();
                break;

            case KeyCode.Escape:
                Close();
                e.Use();
                break;
        }
    }

    private void OpenScene(int index)
    {
        if (index < 0 || index >= _scenePaths.Length) return;

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(_scenePaths[index]);
            Close();
        }
    }
}
