using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 직렬화 가능한 키-밸류 쌍
/// Inspector에서 가로로 표시됨
/// </summary>
[Serializable]
public struct SerializableKeyValuePair<TKey, TValue>
{
    public TKey key;
    public TValue value;

    public SerializableKeyValuePair(TKey key, TValue value)
    {
        this.key = key;
        this.value = value;
    }
}

/// <summary>
/// Unity Inspector에서 직렬화 가능한 제네릭 딕셔너리
/// ISerializationCallbackReceiver를 통해 직렬화/역직렬화 처리
/// </summary>
[Serializable]
public class UnityDictionary<TKey, TValue> : ISerializationCallbackReceiver,
    IEnumerable<KeyValuePair<TKey, TValue>>
{
    // 직렬화용 리스트
    [SerializeField]
    private List<SerializableKeyValuePair<TKey, TValue>> _serializedList =
        new List<SerializableKeyValuePair<TKey, TValue>>();

    // 런타임 딕셔너리
    private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

    // 코드에서 딕셔너리 변경 시 직렬화 리스트 동기화 플래그
    [NonSerialized]
    private bool _isDirty = false;

    #region 생성자

    public UnityDictionary()
    {
        _dictionary = new Dictionary<TKey, TValue>();
    }

    public UnityDictionary(int capacity)
    {
        _dictionary = new Dictionary<TKey, TValue>(capacity);
    }

    public UnityDictionary(IDictionary<TKey, TValue> dictionary)
    {
        _dictionary = new Dictionary<TKey, TValue>(dictionary);
    }

    #endregion

    #region ISerializationCallbackReceiver

    // 직렬화 전: 코드에서 변경된 경우에만 Dictionary -> List 동기화
    public void OnBeforeSerialize()
    {
        if (_isDirty)
        {
            _serializedList.Clear();
            foreach (var pair in _dictionary)
            {
                _serializedList.Add(new SerializableKeyValuePair<TKey, TValue>(pair.Key, pair.Value));
            }
            _isDirty = false;
        }
    }

    // 역직렬화 후: List -> Dictionary 변환
    public void OnAfterDeserialize()
    {
        _dictionary = new Dictionary<TKey, TValue>();
        foreach (var pair in _serializedList)
        {
            // 중복 키 방지
            if (pair.key != null && !_dictionary.ContainsKey(pair.key))
            {
                _dictionary[pair.key] = pair.value;
            }
        }
    }

    #endregion

    #region Dictionary API

    // 인덱서
    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set
        {
            _dictionary[key] = value;
            _isDirty = true;
        }
    }

    // 프로퍼티
    public int Count => _dictionary.Count;
    public Dictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;
    public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

    // 추가
    public void Add(TKey key, TValue value)
    {
        _dictionary.Add(key, value);
        _isDirty = true;
    }

    // 삭제
    public bool Remove(TKey key)
    {
        if (_dictionary.Remove(key))
        {
            _isDirty = true;
            return true;
        }
        return false;
    }

    // 전체 삭제
    public void Clear()
    {
        _dictionary.Clear();
        _isDirty = true;
    }

    // 키 존재 확인
    public bool ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    // 값 존재 확인
    public bool ContainsValue(TValue value)
    {
        return _dictionary.ContainsValue(value);
    }

    // 값 조회 시도
    public bool TryGetValue(TKey key, out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    #endregion

    #region IEnumerable

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region 유틸리티

    // Dictionary로 변환
    public Dictionary<TKey, TValue> ToDictionary()
    {
        return new Dictionary<TKey, TValue>(_dictionary);
    }

    // Dictionary에서 복사
    public void CopyFrom(IDictionary<TKey, TValue> source)
    {
        _dictionary.Clear();
        foreach (var pair in source)
        {
            _dictionary[pair.Key] = pair.Value;
        }
        _isDirty = true;
    }

    #endregion
}
