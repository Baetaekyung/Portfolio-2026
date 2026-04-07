using System;
using System.Collections.Generic;

/*
RedDot 트리의 개별 노드
리프 노드는 조건 함수로 상태를 판단하고,
비-리프 노드는 자식 상태를 집계하여 판단
*/
public class RedDotNode
{
    private readonly ERedDotKey _key;
    private readonly RedDotNode _parent;
    private readonly List<RedDotNode> _children = new();

    private bool _isActive;
    private bool _isDirty;
    private Func<bool> _condition;
    private Action<bool> _onValueChanged;

    public ERedDotKey Key => _key;
    public RedDotNode Parent => _parent;
    public bool IsActive => _isActive;
    public bool IsDirty => _isDirty;
    public bool IsLeaf => _children.Count == 0;
    public int Depth { get; }

    public RedDotNode(ERedDotKey key, RedDotNode parent = null)
    {
        _key = key;
        _parent = parent;
        Depth = _parent != null ? _parent.Depth + 1 : 0;

        // 부모 노드에 자식으로 등록
        _parent?._children.Add(this);
    }

    // 리프 노드용 조건 함수 등록
    public void SetCondition(Func<bool> condition)
    {
        _condition = condition;
    }

    // UI 바인딩용 리스너 등록 (등록 즉시 현재 상태 알림)
    public void AddListener(Action<bool> listener)
    {
        _onValueChanged += listener;
        listener?.Invoke(_isActive);
    }

    // UI 바인딩 해제
    public void RemoveListener(Action<bool> listener)
    {
        _onValueChanged -= listener;
    }

    // Dirty 상태 설정
    public void SetDirty(bool dirty)
    {
        _isDirty = dirty;
    }

    // 노드 상태 평가
    public void Evaluate()
    {
        _isDirty = false;

        var newValue = IsLeaf
            ? _condition != null && _condition.Invoke()
            : EvaluateChildren();

        // 상태 변경 시에만 이벤트 발행
        if (newValue == _isActive) return;

        _isActive = newValue;
        _onValueChanged?.Invoke(_isActive);
    }

    // 자식 중 하나라도 활성이면 true
    private bool EvaluateChildren()
    {
        foreach (var child in _children)
        {
            if (child._isActive) return true;
        }
        return false;
    }
}
