using System;
using System.Collections.Generic;
using UnityEngine;

/*
RedDot 트리 구조를 관리하는 중앙 매니저
Dirty Flag 기반으로 LateUpdate에서 일괄 평가
*/
[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public class RedDotConditionManager : Singleton<RedDotConditionManager>
{
    private readonly Dictionary<ERedDotKey, RedDotNode> _nodeMap = new();
    private readonly HashSet<RedDotNode> _dirtyNodes = new();
    private readonly List<RedDotNode> _sortBuffer = new();

    private RedDotNode _root;

    protected override void OnCreated()
    {
        base.OnCreated();
        InitTree();
    }

    // 트리 구조 초기화
    private void InitTree()
    {
        _root = CreateNode(ERedDotKey.ROOT);

        CreateNode(ERedDotKey.GET_NEW_CHARACTER, ERedDotKey.ROOT);
        CreateNode(ERedDotKey.UPDATE_PROFILE, ERedDotKey.GET_NEW_CHARACTER);

        /*
        아래에 트리 구조를 정의합니다.
        예시:
        CreateNode(ERedDotKey.SHOP, ERedDotKey.ROOT);
        CreateNode(ERedDotKey.SHOP_WEAPON, ERedDotKey.SHOP);
        CreateNode(ERedDotKey.SHOP_ARMOR, ERedDotKey.SHOP);
        CreateNode(ERedDotKey.MAIL, ERedDotKey.ROOT);
        */
    }

    // 루트 노드 생성
    private RedDotNode CreateNode(ERedDotKey key)
    {
        var node = new RedDotNode(key);
        _nodeMap.Add(key, node);
        return node;
    }

    // 자식 노드 생성
    private RedDotNode CreateNode(ERedDotKey key, ERedDotKey parentKey)
    {
        if (!_nodeMap.TryGetValue(parentKey, out var parent))
        {
            Debug.LogError($"[RedDot] 부모 노드를 찾을 수 없습니다: {parentKey}");
            return null;
        }

        var node = new RedDotNode(key, parent);
        _nodeMap.Add(key, node);
        return node;
    }

    // 리프 노드에 조건 함수 등록
    public void RegisterCondition(ERedDotKey key, Func<bool> condition)
    {
        if (!_nodeMap.TryGetValue(key, out var node))
        {
            Debug.LogError($"[RedDot] 노드를 찾을 수 없습니다: {key}");
            return;
        }

        node.SetCondition(condition);
    }

    // 조건 함수 해제
    public void UnregisterCondition(ERedDotKey key)
    {
        if (_nodeMap.TryGetValue(key, out var node))
        {
            node.SetCondition(null);
        }
    }

    // 특정 노드를 Dirty로 마킹 (해당 노드부터 루트까지 전부 마킹)
    public void MarkDirty(ERedDotKey key)
    {
        if (!_nodeMap.TryGetValue(key, out var node))
        {
            Debug.LogError($"[RedDot] 노드를 찾을 수 없습니다: {key}");
            return;
        }

        // 해당 노드부터 루트까지 조상 체인 전체를 dirty 마킹
        var current = node;
        while (current != null)
        {
            if (!_dirtyNodes.Add(current)) break;
            current = current.Parent;
        }
    }

    // 전체 노드 Dirty 마킹
    public void MarkAllDirty()
    {
        foreach (var node in _nodeMap.Values)
        {
            _dirtyNodes.Add(node);
        }
    }

    // UI 리스너 등록
    public void AddListener(ERedDotKey key, Action<bool> listener)
    {
        if (!_nodeMap.TryGetValue(key, out var node))
        {
            Debug.LogError($"[RedDot] 노드를 찾을 수 없습니다: {key}");
            return;
        }

        node.AddListener(listener);
    }

    // UI 리스너 해제
    public void RemoveListener(ERedDotKey key, Action<bool> listener)
    {
        if (_nodeMap.TryGetValue(key, out var node))
        {
            node.RemoveListener(listener);
        }
    }

    // 특정 노드의 활성 상태 조회
    public bool IsActive(ERedDotKey key)
    {
        return _nodeMap.TryGetValue(key, out var node) && node.IsActive;
    }

    private void LateUpdate()
    {
        if (_dirtyNodes.Count == 0) return;

        // Dirty 노드를 깊이 내림차순 정렬 (리프 우선 평가)
        _sortBuffer.Clear();
        _sortBuffer.AddRange(_dirtyNodes);
        _sortBuffer.Sort((a, b) => b.Depth.CompareTo(a.Depth));
        _dirtyNodes.Clear();

        // 정렬된 순서로 평가 (리프 → 루트)
        foreach (var node in _sortBuffer)
        {
            node.Evaluate();
        }
    }
}
