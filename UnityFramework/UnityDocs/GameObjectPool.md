# GameObjectPool

## 개요
Unity GameObject 전용 풀링 시스템입니다.
**AssetLoader(Addressables) 기반**으로 프리팹을 로드하며, **LRU(Least Recently Used) 방식**으로 메모리를 효율적으로 관리합니다.

> **참고**: GameObjectPool은 더 이상 싱글톤이 아닙니다. `PoolManager`를 통해 카테고리별로 접근합니다.
> 자세한 내용은 [PoolManager.md](PoolManager.md)를 참조하세요.

## 클래스 정보
- **위치**: `Assets/Script/Common/Pool/GameObjectPool.cs`
- **상속**: `MonoBehaviour`
- **의존성**: `AssetLoader`, `ObjectPool<T>`
- **관리**: `PoolManager`에 의해 카테고리별로 생성 및 관리

## 핵심 기능

### Addressables 기반 풀링
```
1. Addressables 키로 프리팹 등록 → AssetLoader를 통해 로드
2. Spawn 요청 → 풀에서 가져오기 (없으면 Instantiate)
3. Despawn 요청 → 풀에 반환 (SetActive false)
4. LRU 정리 → 오래된 오브젝트 자동 Destroy
```

### 계층 구조 관리
```
[PoolManager]
└── [PoolContainers]
    └── [Pool_IN_GAME_OBJECT]  (카테고리별 GameObjectPool)
        └── [PoolContainers]
            ├── [Pool] BulletPrefab
            │   ├── Bullet(Clone) - Pooled
            │   ├── Bullet(Clone) - Pooled
            │   └── Bullet(Clone) - Active (씬에서 사용 중)
            └── [Pool] EnemyPrefab
                ├── Enemy(Clone) - Pooled
                └── Enemy(Clone) - Pooled
```

## 데이터 구조

### GameObjectPoolConfig
```csharp
[Serializable]
public class GameObjectPoolConfig
{
    public GameObject Prefab;         // 직접 프리팹 참조 (선택)
    public string AddressableKey;     // Addressables 키 (권장)
    public int InitialSize = 5;       // 초기 생성 개수
    public int MaxSize = 50;          // 최대 크기
    public float AutoShrinkTime = 30f;// 자동 정리 주기 (초)
    public float ExpireTime = 60f;    // 미사용 만료 시간 (초)
}
```

## 사용 예시

> **참고**: GameObjectPool은 `PoolManager`를 통해 접근합니다.
> 적절한 카테고리를 선택하여 사용하세요. (UI, Effect, InGameObject, Audio)

### Addressables 키로 사용 (권장)
```csharp
// 동기 등록 (InGameObject 카테고리 예시)
PoolManager.Instance.InGameObject.RegisterPrefab("Prefabs/Bullet", new GameObjectPoolConfig
{
    InitialSize = 20,
    MaxSize = 100
});

// 비동기 등록
PoolManager.Instance.InGameObject.RegisterPrefabAsync("Prefabs/Enemy", config, () =>
{
    Debug.Log("Enemy 풀 준비 완료");
});

// Spawn
var bullet = PoolManager.Instance.InGameObject.Spawn("Prefabs/Bullet", position, rotation);

// Despawn
PoolManager.Instance.InGameObject.Despawn(bullet);
```

### 프리팹 직접 참조 (기존 방식 호환)
```csharp
// 프리팹 등록
PoolManager.Instance.InGameObject.RegisterPrefab(bulletPrefab, new GameObjectPoolConfig
{
    InitialSize = 20,
    MaxSize = 100
});

// Spawn
var bullet = PoolManager.Instance.InGameObject.Spawn(bulletPrefab, position, rotation);

// Despawn
PoolManager.Instance.InGameObject.Despawn(bullet);
```

### Inspector 설정
```csharp
public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private GameObjectPoolConfig bulletConfig;

    private void Start()
    {
        // AddressableKey 또는 Prefab 중 하나만 설정
        PoolManager.Instance.InGameObject.RegisterPrefab(bulletConfig);
    }

    public void Fire()
    {
        var bullet = PoolManager.Instance.InGameObject.Spawn(
            bulletConfig.AddressableKey,
            transform.position,
            transform.rotation
        );
    }
}
```

### 자동 반환 (시간 기반)
```csharp
// 3초 후 자동 Despawn
var bullet = PoolManager.Instance.InGameObject.Spawn("Prefabs/Bullet", pos, rot);
PoolManager.Instance.InGameObject.DespawnDelayed(bullet, 3f);
```

### 컴포넌트와 함께 Spawn
```csharp
// Addressables 키로 Spawn
var enemy = PoolManager.Instance.InGameObject.Spawn<EnemyController>(
    "Prefabs/Enemy", pos, rot
);
enemy.Initialize(targetPlayer);

// 프리팹으로 Spawn
var bullet = PoolManager.Instance.InGameObject.Spawn<Bullet>(
    bulletPrefab, pos, rot
);
```

## API 레퍼런스

### 풀 관리
| 메서드 | 설명 |
|--------|------|
| `RegisterPrefab(key, config)` | Addressables 키로 풀 등록 (동기) |
| `RegisterPrefabAsync(key, config, onComplete)` | Addressables 키로 풀 등록 (비동기) |
| `RegisterPrefab(prefab, config)` | 프리팹 직접 참조로 풀 등록 |
| `RegisterPrefab(config)` | Config 기반 풀 등록 |
| `UnregisterPrefab(key)` | Addressables 키로 풀 해제 |
| `UnregisterPrefab(prefab)` | 프리팹으로 풀 해제 |
| `HasPool(key)` | 풀 존재 여부 확인 (키) |
| `HasPool(prefab)` | 풀 존재 여부 확인 (프리팹) |
| `GetPoolInfo(key)` | 풀 상태 정보 조회 (키) |
| `GetPoolInfo(prefab)` | 풀 상태 정보 조회 (프리팹) |

### Spawn/Despawn
| 메서드 | 설명 |
|--------|------|
| `Spawn(key)` | Addressables 키로 기본 위치 Spawn |
| `Spawn(key, pos, rot)` | Addressables 키로 지정 위치 Spawn |
| `Spawn(key, parent)` | Addressables 키로 부모 아래 Spawn |
| `Spawn<T>(key, pos, rot)` | 컴포넌트와 함께 Spawn (키) |
| `Spawn(prefab)` | 프리팹으로 기본 위치 Spawn |
| `Spawn(prefab, pos, rot)` | 프리팹으로 지정 위치 Spawn |
| `Spawn<T>(prefab, pos, rot)` | 컴포넌트와 함께 Spawn (프리팹) |
| `Despawn(obj)` | 오브젝트 반환 |
| `DespawnDelayed(obj, delay)` | 지연 반환 |
| `DespawnAll(key)` | 키 기준 모든 활성 오브젝트 반환 |
| `DespawnAll(prefab)` | 프리팹 기준 모든 활성 오브젝트 반환 |

### 정리
| 메서드 | 설명 |
|--------|------|
| `Shrink(key)` | 특정 풀 LRU 정리 (키) |
| `Shrink(prefab)` | 특정 풀 LRU 정리 (프리팹) |
| `ShrinkAll()` | 모든 풀 LRU 정리 |
| `Clear(key)` | 특정 풀 완전 정리 (키) |
| `Clear(prefab)` | 특정 풀 완전 정리 (프리팹) |
| `ClearAll()` | 모든 풀 완전 정리 |
| `Warmup(key, count)` | 미리 오브젝트 생성 (키) |
| `Warmup(prefab, count)` | 미리 오브젝트 생성 (프리팹) |

## IPoolable 인터페이스

풀링 이벤트를 받고 싶은 컴포넌트용 인터페이스:

```csharp
public interface IPoolable
{
    void OnSpawn();     // Spawn 시 호출
    void OnDespawn();   // Despawn 시 호출
}
```

### 사용 예시
```csharp
public class Bullet : MonoBehaviour, GameObjectPool.IPoolable
{
    private Rigidbody _rb;

    public void OnSpawn()
    {
        // 초기화
        _rb.velocity = Vector3.zero;
    }

    public void OnDespawn()
    {
        // 정리
        StopAllCoroutines();
    }
}
```

## 자동 정리 (LRU)

### 정리 조건
1. `shrinkCheckInterval` 주기마다 자동 실행
2. `ExpireTime`보다 오래 사용되지 않은 오브젝트
3. 현재 풀 크기가 `InitialSize`보다 클 때

### 정리 순서
```
1. 풀에서 대기 중인 오브젝트들의 마지막 사용 시간 확인
2. ExpireTime 초과한 오브젝트들을 LRU 순서로 정렬
3. MaxSize 또는 InitialSize까지 Destroy
```

## Inspector 설정

```csharp
[Header("사전 로드 풀")]
[SerializeField] private List<GameObjectPoolConfig> preloadPools;

[Header("자동 정리 설정")]
[SerializeField] private bool autoShrink = true;
[SerializeField] private float shrinkCheckInterval = 10f;
```

## 성능 팁

1. **비동기 등록 사용**: 로딩 화면에서 `RegisterPrefabAsync` 사용
   ```csharp
   PoolManager.Instance.InGameObject.RegisterPrefabAsync("Prefabs/Enemy", config, OnLoadComplete);
   ```

2. **Warmup 사용**: 게임 시작 시 미리 생성
   ```csharp
   PoolManager.Instance.InGameObject.Warmup("Prefabs/Bullet", 50);
   ```

3. **적절한 MaxSize 설정**: 너무 크면 메모리 낭비, 너무 작으면 GC 발생

4. **IPoolable 활용**: Awake/Start 대신 OnSpawn에서 초기화

5. **DespawnDelayed 활용**: 파티클 등 지연 반환 필요 시

## 주의사항

- Despawn 시 오브젝트가 비활성화됨 (`SetActive(false)`)
- `PoolManager`가 씬 전환 시에도 유지됨 (`DontDestroyOnLoad`)
- Addressables 키가 잘못되면 로드 실패 로그 출력
- `OnDestroy`에서 Despawn 호출 금지 (무한 루프 위험)
- `AssetLoader`가 먼저 초기화되어 있어야 함
- 더 이상 `GameObjectPool.Instance`로 접근 불가, `PoolManager`를 통해 접근
