using System;
using System.Collections.Generic;
using UnityEngine;

/*
GoogleSpreadSheet에서 불러온 데이터를 저장하는 ScriptableObject 기반 테이블 클래스
각 테이블은 이 클래스를 상속받아 자신만의 Row 타입을 정의
*/
public abstract class BaseTable<TRow> : ScriptableObject
    where TRow : BaseTable<TRow>.Row
{
    // 모든 테이블 Row의 기본 클래스
    [Serializable]
    public abstract class Row
    {
        // 모든 Row는 고유 ID를 가져야 함 (델타 업데이트용)
        public int id;
    }

    [Header("버전 정보")]
    [SerializeField]
    protected string version = "1.0.0";

    [SerializeField]
    protected List<TRow> rows = new List<TRow>();

    // ID 기반 빠른 검색을 위한 캐시
    private Dictionary<int, TRow> _idCache;
    private bool _isCacheDirty = true;

    // 테이블 버전
    public string Version => version;

    // 외부에서 읽기 전용으로 행 데이터 접근
    public IReadOnlyList<TRow> Rows => rows;

    // 테이블의 총 행 개수
    public int Count => rows.Count;

    // 인덱스로 특정 행 데이터 반환
    public TRow GetRow(int index)
    {
        if (index < 0 || index >= rows.Count)
            return null;

        return rows[index];
    }

    // ID로 특정 행 데이터 반환
    public TRow GetRowById(int id)
    {
        EnsureCache();
        return _idCache.TryGetValue(id, out var row) ? row : null;
    }

    // ID로 행의 인덱스 반환
    public int GetRowIndexById(int id)
    {
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].id == id)
                return i;
        }
        return -1;
    }

    // ID 캐시 갱신
    private void EnsureCache()
    {
        if (_isCacheDirty || _idCache == null)
        {
            _idCache = new Dictionary<int, TRow>();
            foreach (var row in rows)
            {
                _idCache[row.id] = row;
            }
            _isCacheDirty = false;
        }
    }

    // 캐시 무효화
    private void InvalidateCache()
    {
        _isCacheDirty = true;
    }

    /*
    런타임에서 테이블 데이터를 전체 교체하는 HotFix 메서드
    서버에서 받은 데이터로 테이블을 갱신할 때 사용
    */
    public void HotFix(List<TRow> newRows, string newVersion = null)
    {
        rows = newRows ?? new List<TRow>();
        if (!string.IsNullOrEmpty(newVersion))
            version = newVersion;
        InvalidateCache();
    }

    /*
    델타 업데이트 적용
    추가/수정/삭제된 행만 반영
    */
    public void HotFixDelta(List<TRow> addedRows, List<TRow> modifiedRows, List<int> deletedIds, string newVersion)
    {
        // 삭제 처리
        if (deletedIds != null && deletedIds.Count > 0)
        {
            rows.RemoveAll(row => deletedIds.Contains(row.id));
        }

        // 수정 처리
        if (modifiedRows != null)
        {
            foreach (var modifiedRow in modifiedRows)
            {
                var index = GetRowIndexById(modifiedRow.id);
                if (index >= 0)
                {
                    rows[index] = modifiedRow;
                }
            }
        }

        // 추가 처리
        if (addedRows != null)
        {
            rows.AddRange(addedRows);
        }

        // 버전 업데이트
        if (!string.IsNullOrEmpty(newVersion))
            version = newVersion;

        InvalidateCache();
    }

    // 특정 ID의 행 데이터를 교체
    public void HotFixRowById(int id, TRow newRow)
    {
        var index = GetRowIndexById(id);
        if (index >= 0)
        {
            rows[index] = newRow;
            InvalidateCache();
        }
    }

    // 새로운 행 추가
    public void HotFixAdd(TRow newRow)
    {
        rows.Add(newRow);
        InvalidateCache();
    }

    // 특정 ID의 행 삭제
    public void HotFixRemoveById(int id)
    {
        var index = GetRowIndexById(id);
        if (index >= 0)
        {
            rows.RemoveAt(index);
            InvalidateCache();
        }
    }

    // 버전만 업데이트
    public void UpdateVersion(string newVersion)
    {
        version = newVersion;
    }

#if UNITY_EDITOR
    /*
    TableParser에서 파싱한 데이터를 설정
    에디터에서만 사용 가능
    */
    public void SetRows(List<TRow> newRows)
    {
        rows = newRows;
        UnityEditor.EditorUtility.SetDirty(this);
    }

    // 모든 행 데이터 삭제
    public void ClearRows()
    {
        rows.Clear();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
