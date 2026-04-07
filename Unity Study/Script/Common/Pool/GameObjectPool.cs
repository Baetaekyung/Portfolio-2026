using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectQ.Pool;

/*
Unity GameObject 전용 풀링 시스템
AssetLoader(Addressables) 기반으로 프리팹 로드
LRU 방식으로 메모리 효율적 관리
PoolManager에 의해 카테고리별로 생성되어 관리됨
*/
public class GameObjectPool : MonoBehaviour
{
    // 풀링 이벤트 인터페이스
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }

    // 풀 아이템 정보
    private class PoolItem
    {
        public GameObject Object;
        public GameObject Prefab;
        public float LastUsedTime;
        public bool IsActive;
    }

    // 프리팹별 풀 컨테이너
    private class PrefabPool
    {
        public GameObject Prefab;
        public string AddressableKey;
        public GameObjectPoolConfig Config;
        public Transform Container;
        public List<PoolItem> AllItems = new List<PoolItem>();
        public Dictionary<GameObject, PoolItem> ItemLookup = new Dictionary<GameObject, PoolItem>();
        public LinkedList<PoolItem> AvailableItems = new LinkedList<PoolItem>();
        public int ActiveCount;
    }

    // 풀 카테고리
    public EPoolCategory Category { get; private set; }

    [Header("사전 로드 풀")]
    [SerializeField]
    private List<GameObjectPoolConfig> preloadPools = new List<GameObjectPoolConfig>();

    [Header("자동 정리 설정")]
    [SerializeField]
    private bool autoShrink = true;

    [SerializeField]
    private float shrinkCheckInterval = 10f;

    // 프리팹별 풀 관리
    private readonly Dictionary<GameObject, PrefabPool> _pools = new Dictionary<GameObject, PrefabPool>();

    // Addressables 키 → 프리팹 매핑
    private readonly Dictionary<string, GameObject> _keyToPrefab = new Dictionary<string, GameObject>();

    // 활성 오브젝트 → 프리팹 매핑 (Despawn 시 사용)
    private readonly Dictionary<GameObject, GameObject> _activeObjectToPrefab = new Dictionary<GameObject, GameObject>();

    // 지연 Despawn 코루틴 관리
    private readonly Dictionary<GameObject, Coroutine> _delayedDespawnCoroutines = new Dictionary<GameObject, Coroutine>();

    private Transform _poolRoot;
    private Coroutine _autoShrinkCoroutine;

    // PoolManager에서 호출하는 초기화 메서드
    public void Initialize(EPoolCategory category, CategoryPoolSettings settings = null)
    {
        Category = category;

        // 설정 적용
        if (settings != null)
        {
            autoShrink = settings.autoShrink;
            shrinkCheckInterval = settings.shrinkCheckInterval;
            preloadPools = settings.preloadPools ?? new List<GameObjectPoolConfig>();
        }

        InitializeInternal();
    }

    // 내부 초기화 처리
    private void InitializeInternal()
    {
        // 풀 루트 생성
        _poolRoot = new GameObject("[PoolContainers]").transform;
        _poolRoot.SetParent(transform);

        // 사전 설정된 풀 등록
        foreach (var config in preloadPools)
        {
            if (config.Prefab != null)
            {
                RegisterPrefab(config);
            }
            else if (!string.IsNullOrEmpty(config.AddressableKey))
            {
                RegisterPrefab(config.AddressableKey, config);
            }
        }

        // 자동 정리 시작
        if (autoShrink)
        {
            _autoShrinkCoroutine = StartCoroutine(AutoShrinkRoutine());
        }
    }

    private void OnDestroy()
    {
        if (_autoShrinkCoroutine != null)
        {
            StopCoroutine(_autoShrinkCoroutine);
        }
    }

    #region 풀 관리

    // Addressables 키로 프리팹 풀 등록
    public void RegisterPrefab(string addressableKey, GameObjectPoolConfig config = null)
    {
        if (string.IsNullOrEmpty(addressableKey))
            return;

        // 이미 등록된 키인지 확인
        if (_keyToPrefab.ContainsKey(addressableKey))
            return;

        // AssetLoader를 통해 프리팹 로드
        var prefab = AssetLoader.Instance.Load<GameObject>(addressableKey);
        if (prefab == null)
        {
            Debug.LogError($"[GameObjectPool] 프리팹 로드 실패: {addressableKey}");
            return;
        }

        _keyToPrefab[addressableKey] = prefab;

        config ??= new GameObjectPoolConfig { AddressableKey = addressableKey };
        config.AddressableKey = addressableKey;

        RegisterPrefabInternal(prefab, config, addressableKey);
    }

    // Addressables 키로 비동기 프리팹 풀 등록
    public void RegisterPrefabAsync(string addressableKey, GameObjectPoolConfig config = null, Action onComplete = null)
    {
        if (string.IsNullOrEmpty(addressableKey))
        {
            onComplete?.Invoke();
            return;
        }

        // 이미 등록된 키인지 확인
        if (_keyToPrefab.ContainsKey(addressableKey))
        {
            onComplete?.Invoke();
            return;
        }

        // AssetLoader를 통해 비동기 프리팹 로드
        AssetLoader.Instance.LoadAsync<GameObject>(addressableKey, prefab =>
        {
            if (prefab == null)
            {
                Debug.LogError($"[GameObjectPool] 프리팹 로드 실패: {addressableKey}");
                onComplete?.Invoke();
                return;
            }

            _keyToPrefab[addressableKey] = prefab;

            config ??= new GameObjectPoolConfig { AddressableKey = addressableKey };
            config.AddressableKey = addressableKey;

            RegisterPrefabInternal(prefab, config, addressableKey);
            onComplete?.Invoke();
        });
    }

    // 프리팹으로 직접 풀 등록 (기존 방식 호환)
    public void RegisterPrefab(GameObject prefab, GameObjectPoolConfig config = null)
    {
        if (prefab == null)
            return;

        if (_pools.ContainsKey(prefab))
            return;

        config ??= new GameObjectPoolConfig { Prefab = prefab };
        RegisterPrefabInternal(prefab, config, null);
    }

    // 설정으로 프리팹 풀 등록
    public void RegisterPrefab(GameObjectPoolConfig config)
    {
        if (config == null)
            return;

        if (!string.IsNullOrEmpty(config.AddressableKey))
        {
            RegisterPrefab(config.AddressableKey, config);
        }
        else if (config.Prefab != null)
        {
            RegisterPrefab(config.Prefab, config);
        }
    }

    // 내부 풀 등록 처리
    private void RegisterPrefabInternal(GameObject prefab, GameObjectPoolConfig config, string addressableKey)
    {
        if (_pools.ContainsKey(prefab))
            return;

        var pool = new PrefabPool
        {
            Prefab = prefab,
            AddressableKey = addressableKey,
            Config = config
        };

        // 컨테이너 생성
        var containerObj = new GameObject($"[Pool] {prefab.name}");
        containerObj.transform.SetParent(_poolRoot);
        pool.Container = containerObj.transform;

        _pools[prefab] = pool;

        // 초기 오브젝트 생성
        Warmup(prefab, config.InitialSize);
    }

    // Addressables 키로 프리팹 풀 해제
    public void UnregisterPrefab(string addressableKey)
    {
        if (string.IsNullOrEmpty(addressableKey))
            return;

        if (!_keyToPrefab.TryGetValue(addressableKey, out var prefab))
            return;

        UnregisterPrefab(prefab);
        _keyToPrefab.Remove(addressableKey);
    }

    // 프리팹 풀 해제
    public void UnregisterPrefab(GameObject prefab)
    {
        if (prefab == null || !_pools.TryGetValue(prefab, out var pool))
            return;

        // 모든 아이템 정리
        foreach (var item in pool.AllItems)
        {
            if (item.Object != null)
            {
                Destroy(item.Object);
            }
        }

        // 컨테이너 정리
        if (pool.Container != null)
        {
            Destroy(pool.Container.gameObject);
        }

        // Addressables 키 매핑 제거
        if (!string.IsNullOrEmpty(pool.AddressableKey))
        {
            _keyToPrefab.Remove(pool.AddressableKey);
        }

        _pools.Remove(prefab);
    }

    // 풀 존재 여부 확인 (키)
    public bool HasPool(string addressableKey)
    {
        return !string.IsNullOrEmpty(addressableKey) && _keyToPrefab.ContainsKey(addressableKey);
    }

    // 풀 존재 여부 확인 (프리팹)
    public bool HasPool(GameObject prefab)
    {
        return prefab != null && _pools.ContainsKey(prefab);
    }

    // 풀 상태 정보 조회 (키)
    public (int active, int pooled, int total) GetPoolInfo(string addressableKey)
    {
        if (string.IsNullOrEmpty(addressableKey) || !_keyToPrefab.TryGetValue(addressableKey, out var prefab))
            return (0, 0, 0);

        return GetPoolInfo(prefab);
    }

    // 풀 상태 정보 조회 (프리팹)
    public (int active, int pooled, int total) GetPoolInfo(GameObject prefab)
    {
        if (prefab == null || !_pools.TryGetValue(prefab, out var pool))
            return (0, 0, 0);

        return (pool.ActiveCount, pool.AvailableItems.Count, pool.AllItems.Count);
    }

    #endregion

    #region Spawn

    // Addressables 키로 Spawn
    public GameObject Spawn(string addressableKey)
    {
        return Spawn(addressableKey, Vector3.zero, Quaternion.identity, null);
    }

    // Addressables 키로 위치/회전 지정 Spawn
    public GameObject Spawn(string addressableKey, Vector3 position, Quaternion rotation)
    {
        return Spawn(addressableKey, position, rotation, null);
    }

    // Addressables 키로 부모 지정 Spawn
    public GameObject Spawn(string addressableKey, Transform parent)
    {
        return Spawn(addressableKey, Vector3.zero, Quaternion.identity, parent);
    }

    // Addressables 키로 전체 옵션 Spawn
    public GameObject Spawn(string addressableKey, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (string.IsNullOrEmpty(addressableKey))
            return null;

        // 풀이 없으면 자동 등록
        if (!_keyToPrefab.TryGetValue(addressableKey, out var prefab))
        {
            RegisterPrefab(addressableKey);
            if (!_keyToPrefab.TryGetValue(addressableKey, out prefab))
                return null;
        }

        return SpawnInternal(prefab, position, rotation, parent);
    }

    // Addressables 키로 컴포넌트와 함께 Spawn
    public T Spawn<T>(string addressableKey, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        var obj = Spawn(addressableKey, position, rotation, parent);
        return obj != null ? obj.GetComponent<T>() : null;
    }

    // 기본 Spawn (프리팹)
    public GameObject Spawn(GameObject prefab)
    {
        return Spawn(prefab, Vector3.zero, Quaternion.identity, null);
    }

    // 위치/회전 지정 Spawn (프리팹)
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Spawn(prefab, position, rotation, null);
    }

    // 부모 지정 Spawn (프리팹)
    public GameObject Spawn(GameObject prefab, Transform parent)
    {
        return Spawn(prefab, Vector3.zero, Quaternion.identity, parent);
    }

    // 전체 옵션 Spawn (프리팹)
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null)
            return null;

        // 풀이 없으면 자동 등록
        if (!_pools.ContainsKey(prefab))
        {
            RegisterPrefab(prefab);
        }

        return SpawnInternal(prefab, position, rotation, parent);
    }

    // 내부 Spawn 처리
    private GameObject SpawnInternal(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        var pool = _pools[prefab];
        PoolItem item;

        if (pool.AvailableItems.Count > 0)
        {
            // 가장 최근에 반환된 것 사용 (캐시 효율)
            item = pool.AvailableItems.Last.Value;
            pool.AvailableItems.RemoveLast();
        }
        else if (pool.AllItems.Count < pool.Config.MaxSize)
        {
            // 새로 생성
            item = CreateItem(pool);
        }
        else
        {
            // 최대 크기 도달
            Debug.LogWarning($"[GameObjectPool] Pool '{prefab.name}' reached max size ({pool.Config.MaxSize})");
            item = CreateItem(pool);
        }

        // 활성화
        item.IsActive = true;
        item.LastUsedTime = Time.time;
        pool.ActiveCount++;

        var obj = item.Object;
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        _activeObjectToPrefab[obj] = prefab;

        // IPoolable 콜백
        NotifySpawn(obj);

        return obj;
    }

    // 컴포넌트와 함께 Spawn (프리팹)
    public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        var obj = Spawn(prefab, position, rotation, parent);
        return obj != null ? obj.GetComponent<T>() : null;
    }

    #endregion

    #region Despawn

    // 오브젝트 반환
    public void Despawn(GameObject obj)
    {
        if (obj == null)
            return;

        // 지연 Despawn 취소
        CancelDelayedDespawn(obj);

        // 프리팹 찾기
        if (!_activeObjectToPrefab.TryGetValue(obj, out var prefab))
        {
            Debug.LogWarning($"[GameObjectPool] Object '{obj.name}' is not managed by pool");
            return;
        }

        if (!_pools.TryGetValue(prefab, out var pool))
            return;

        if (!pool.ItemLookup.TryGetValue(obj, out var item))
            return;

        if (!item.IsActive)
            return;

        // IPoolable 콜백
        NotifyDespawn(obj);

        // 비활성화
        item.IsActive = false;
        item.LastUsedTime = Time.time;
        pool.ActiveCount--;

        obj.SetActive(false);
        obj.transform.SetParent(pool.Container);

        _activeObjectToPrefab.Remove(obj);

        // LRU: 가장 최근에 반환된 것을 뒤에 추가
        pool.AvailableItems.AddLast(item);

        // 최대 크기 초과 시 정리
        TrimExcess(pool);
    }

    // 지연 Despawn
    public void DespawnDelayed(GameObject obj, float delay)
    {
        if (obj == null)
            return;

        // 기존 지연 취소
        CancelDelayedDespawn(obj);

        var coroutine = StartCoroutine(DespawnDelayedCoroutine(obj, delay));
        _delayedDespawnCoroutines[obj] = coroutine;
    }

    private IEnumerator DespawnDelayedCoroutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        _delayedDespawnCoroutines.Remove(obj);
        Despawn(obj);
    }

    private void CancelDelayedDespawn(GameObject obj)
    {
        if (_delayedDespawnCoroutines.TryGetValue(obj, out var coroutine))
        {
            StopCoroutine(coroutine);
            _delayedDespawnCoroutines.Remove(obj);
        }
    }

    // Addressables 키로 모든 활성 오브젝트 반환
    public void DespawnAll(string addressableKey)
    {
        if (string.IsNullOrEmpty(addressableKey) || !_keyToPrefab.TryGetValue(addressableKey, out var prefab))
            return;

        DespawnAll(prefab);
    }

    // 특정 프리팹의 모든 활성 오브젝트 반환
    public void DespawnAll(GameObject prefab)
    {
        if (prefab == null || !_pools.TryGetValue(prefab, out var pool))
            return;

        var toRelease = new List<PoolItem>();
        foreach (var item in pool.AllItems)
        {
            if (item.IsActive)
            {
                toRelease.Add(item);
            }
        }

        foreach (var item in toRelease)
        {
            Despawn(item.Object);
        }
    }

    #endregion

    #region 정리

    // Addressables 키로 특정 풀 LRU 정리
    public void Shrink(string addressableKey)
    {
        if (string.IsNullOrEmpty(addressableKey) || !_keyToPrefab.TryGetValue(addressableKey, out var prefab))
            return;

        Shrink(prefab);
    }

    // 특정 풀 LRU 정리
    public void Shrink(GameObject prefab)
    {
        if (prefab == null || !_pools.TryGetValue(prefab, out var pool))
            return;

        ShrinkPool(pool);
    }

    // 모든 풀 LRU 정리
    public void ShrinkAll()
    {
        foreach (var pool in _pools.Values)
        {
            ShrinkPool(pool);
        }
    }

    private void ShrinkPool(PrefabPool pool)
    {
        var currentTime = Time.time;
        var toRemove = new List<PoolItem>();

        // 만료된 오브젝트 찾기
        foreach (var item in pool.AvailableItems)
        {
            if (currentTime - item.LastUsedTime > pool.Config.ExpireTime)
            {
                toRemove.Add(item);
            }
        }

        // InitialSize 이하로는 줄이지 않음
        var targetRemoveCount = Math.Min(
            toRemove.Count,
            pool.AllItems.Count - pool.Config.InitialSize
        );

        if (targetRemoveCount <= 0)
            return;

        // LRU 순서로 정렬 (오래된 것 먼저)
        toRemove.Sort((a, b) => a.LastUsedTime.CompareTo(b.LastUsedTime));

        for (int i = 0; i < targetRemoveCount; i++)
        {
            DestroyItem(pool, toRemove[i]);
        }
    }

    // Addressables 키로 특정 풀 완전 정리
    public void Clear(string addressableKey)
    {
        if (string.IsNullOrEmpty(addressableKey) || !_keyToPrefab.TryGetValue(addressableKey, out var prefab))
            return;

        Clear(prefab);
    }

    // 특정 풀 완전 정리
    public void Clear(GameObject prefab)
    {
        if (prefab == null || !_pools.TryGetValue(prefab, out var pool))
            return;

        foreach (var item in pool.AllItems.ToArray())
        {
            if (item.Object != null)
            {
                Destroy(item.Object);
            }
        }

        pool.AllItems.Clear();
        pool.ItemLookup.Clear();
        pool.AvailableItems.Clear();
        pool.ActiveCount = 0;
    }

    // 모든 풀 완전 정리
    public void ClearAll()
    {
        foreach (var prefab in new List<GameObject>(_pools.Keys))
        {
            Clear(prefab);
        }
    }

    // Warmup (프리팹)
    public void Warmup(GameObject prefab, int count)
    {
        if (prefab == null || !_pools.TryGetValue(prefab, out var pool))
            return;

        for (int i = 0; i < count; i++)
        {
            if (pool.AllItems.Count >= pool.Config.MaxSize)
                break;

            var item = CreateItem(pool);
            item.LastUsedTime = Time.time;
            pool.AvailableItems.AddLast(item);
        }
    }

    // Warmup (Addressables 키)
    public void Warmup(string addressableKey, int count)
    {
        if (string.IsNullOrEmpty(addressableKey) || !_keyToPrefab.TryGetValue(addressableKey, out var prefab))
            return;

        Warmup(prefab, count);
    }

    #endregion

    #region 내부 메서드

    private PoolItem CreateItem(PrefabPool pool)
    {
        var obj = Instantiate(pool.Prefab, pool.Container);
        obj.SetActive(false);

        var item = new PoolItem
        {
            Object = obj,
            Prefab = pool.Prefab,
            LastUsedTime = Time.time,
            IsActive = false
        };

        pool.AllItems.Add(item);
        pool.ItemLookup[obj] = item;

        return item;
    }

    private void DestroyItem(PrefabPool pool, PoolItem item)
    {
        if (item.Object != null)
        {
            Destroy(item.Object);
        }

        pool.AvailableItems.Remove(item);
        pool.AllItems.Remove(item);
        pool.ItemLookup.Remove(item.Object);
    }

    private void TrimExcess(PrefabPool pool)
    {
        while (pool.AllItems.Count > pool.Config.MaxSize && pool.AvailableItems.Count > 0)
        {
            // LRU: 가장 오래된 것 제거 (맨 앞)
            var oldest = pool.AvailableItems.First.Value;
            DestroyItem(pool, oldest);
        }
    }

    private void NotifySpawn(GameObject obj)
    {
        var poolables = obj.GetComponents<IPoolable>();
        foreach (var poolable in poolables)
        {
            poolable.OnSpawn();
        }
    }

    private void NotifyDespawn(GameObject obj)
    {
        var poolables = obj.GetComponents<IPoolable>();
        foreach (var poolable in poolables)
        {
            poolable.OnDespawn();
        }
    }

    private IEnumerator AutoShrinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shrinkCheckInterval);
            ShrinkAll();
        }
    }

    // 전체 풀 통계 조회
    public (int active, int pooled, int total) GetTotalStats()
    {
        int totalActive = 0;
        int totalPooled = 0;
        int total = 0;

        foreach (var pool in _pools.Values)
        {
            totalActive += pool.ActiveCount;
            totalPooled += pool.AvailableItems.Count;
            total += pool.AllItems.Count;
        }

        return (totalActive, totalPooled, total);
    }

    #endregion
}

// GameObject 풀 설정
[Serializable]
public class GameObjectPoolConfig
{
    public GameObject Prefab;
    public string AddressableKey;
    public int InitialSize = 5;
    public int MaxSize = 50;
    public float AutoShrinkTime = 30f;
    public float ExpireTime = 60f;
}
