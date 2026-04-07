using System.Collections.Generic;
using UnityEngine;

/*
카테고리별 GameObject 풀을 관리하는 매니저
UI, Effect, InGameObject, Audio 4개의 풀을 독립적으로 관리
*/
[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public class PoolManager : Singleton<PoolManager>
{
    [Header("카테고리별 풀 설정")]
    [SerializeField]
    private CategoryPoolSettings uiSettings = new CategoryPoolSettings();

    [SerializeField]
    private CategoryPoolSettings effectSettings = new CategoryPoolSettings();

    [SerializeField]
    private CategoryPoolSettings inGameObjectSettings = new CategoryPoolSettings();

    [SerializeField]
    private CategoryPoolSettings audioSettings = new CategoryPoolSettings();

    // 카테고리별 풀 인스턴스
    private GameObjectPool _uiPool;
    private GameObjectPool _effectPool;
    private GameObjectPool _inGameObjectPool;
    private GameObjectPool _audioPool;

    // 카테고리 → 풀 매핑
    private Dictionary<EPoolCategory, GameObjectPool> _categoryToPool;

    // 풀 컨테이너 루트
    private Transform _poolRoot;

    // 프로퍼티로 접근
    public GameObjectPool UI => _uiPool;
    public GameObjectPool Effect => _effectPool;
    public GameObjectPool InGameObject => _inGameObjectPool;
    public GameObjectPool Audio => _audioPool;

    protected override void OnCreated()
    {
        base.OnCreated();

        // 풀 루트 생성
        _poolRoot = new GameObject("[PoolContainers]").transform;
        _poolRoot.SetParent(transform);

        // 카테고리별 풀 초기화
        _uiPool = CreateCategoryPool(EPoolCategory.UI, uiSettings);
        _effectPool = CreateCategoryPool(EPoolCategory.EFFECT, effectSettings);
        _inGameObjectPool = CreateCategoryPool(EPoolCategory.IN_GAME_OBJECT, inGameObjectSettings);
        _audioPool = CreateCategoryPool(EPoolCategory.AUDIO, audioSettings);

        // 딕셔너리 매핑
        _categoryToPool = new Dictionary<EPoolCategory, GameObjectPool>
        {
            { EPoolCategory.UI, _uiPool },
            { EPoolCategory.EFFECT, _effectPool },
            { EPoolCategory.IN_GAME_OBJECT, _inGameObjectPool },
            { EPoolCategory.AUDIO, _audioPool }
        };
    }

    // 카테고리별 풀 생성
    private GameObjectPool CreateCategoryPool(EPoolCategory category, CategoryPoolSettings settings)
    {
        var poolObj = new GameObject($"[Pool_{category}]");
        poolObj.transform.SetParent(_poolRoot);

        var pool = poolObj.AddComponent<GameObjectPool>();
        pool.Initialize(category, settings);

        return pool;
    }

    // 카테고리로 풀 접근
    public GameObjectPool GetPool(EPoolCategory category)
    {
        return _categoryToPool.TryGetValue(category, out var pool) ? pool : null;
    }

    #region 단축 API

    // 카테고리 지정 Spawn
    public GameObject Spawn(EPoolCategory category, string key)
    {
        return GetPool(category)?.Spawn(key);
    }

    public GameObject Spawn(EPoolCategory category, string key, Vector3 position, Quaternion rotation)
    {
        return GetPool(category)?.Spawn(key, position, rotation);
    }

    public GameObject Spawn(EPoolCategory category, string key, Vector3 position, Quaternion rotation, Transform parent)
    {
        return GetPool(category)?.Spawn(key, position, rotation, parent);
    }

    public GameObject Spawn(EPoolCategory category, string key, Transform parent)
    {
        return GetPool(category)?.Spawn(key, parent);
    }

    public GameObject Spawn(EPoolCategory category, GameObject prefab)
    {
        return GetPool(category)?.Spawn(prefab);
    }

    public GameObject Spawn(EPoolCategory category, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return GetPool(category)?.Spawn(prefab, position, rotation);
    }

    public GameObject Spawn(EPoolCategory category, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        return GetPool(category)?.Spawn(prefab, position, rotation, parent);
    }

    public GameObject Spawn(EPoolCategory category, GameObject prefab, Transform parent)
    {
        return GetPool(category)?.Spawn(prefab, parent);
    }

    // 컴포넌트 포함 Spawn
    public T Spawn<T>(EPoolCategory category, string key, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        return GetPool(category)?.Spawn<T>(key, position, rotation, parent);
    }

    public T Spawn<T>(EPoolCategory category, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        return GetPool(category)?.Spawn<T>(prefab, position, rotation, parent);
    }

    // 카테고리 지정 Despawn
    public void Despawn(EPoolCategory category, GameObject obj)
    {
        GetPool(category)?.Despawn(obj);
    }

    public void DespawnDelayed(EPoolCategory category, GameObject obj, float delay)
    {
        GetPool(category)?.DespawnDelayed(obj, delay);
    }

    #endregion

    #region 전체 메모리 관리

    // 모든 풀 LRU 정리
    public void ShrinkAll()
    {
        _uiPool.ShrinkAll();
        _effectPool.ShrinkAll();
        _inGameObjectPool.ShrinkAll();
        _audioPool.ShrinkAll();
    }

    // 모든 풀 완전 정리
    public void ClearAll()
    {
        _uiPool?.ClearAll();
        _effectPool?.ClearAll();
        _inGameObjectPool?.ClearAll();
        _audioPool?.ClearAll();
    }

    // 특정 카테고리만 정리
    public void ClearCategory(EPoolCategory category)
    {
        GetPool(category)?.ClearAll();
    }

    // 특정 카테고리 LRU 정리
    public void ShrinkCategory(EPoolCategory category)
    {
        GetPool(category)?.ShrinkAll();
    }

    // 통계 조회
    public PoolManagerStats GetStats()
    {
        return new PoolManagerStats
        {
            uiStats = _uiPool?.GetTotalStats() ?? (0, 0, 0),
            effectStats = _effectPool?.GetTotalStats() ?? (0, 0, 0),
            inGameObjectStats = _inGameObjectPool?.GetTotalStats() ?? (0, 0, 0),
            audioStats = _audioPool?.GetTotalStats() ?? (0, 0, 0)
        };
    }

    #endregion

    private void OnDestroy()
    {
        ClearAll();
    }
}
