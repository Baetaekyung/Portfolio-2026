# PoolManager

## 개요
카테고리별 GameObject 풀을 관리하는 매니저입니다.
**UI, Effect, InGameObject, Audio** 4개의 풀을 독립적으로 관리하며, 각 카테고리별로 메모리를 효율적으로 관리합니다.

## 클래스 정보
- **위치**: `Assets/Script/Common/Pool/PoolManager.cs`
- **상속**: `Singleton<PoolManager>`
- **의존성**: `GameObjectPool`, `AssetLoader`

## 카테고리 구성

| 카테고리 | 설명 | 접근 프로퍼티 |
|---------|------|--------------|
| `UI` | 팝업, 버튼, 리스트 아이템 등 | `PoolManager.Instance.UI` |
| `EFFECT` | 파티클, VFX 등 | `PoolManager.Instance.Effect` |
| `IN_GAME_OBJECT` | 적, 총알, 아이템 등 | `PoolManager.Instance.InGameObject` |
| `AUDIO` | SoundObject 등 오디오 관련 | `PoolManager.Instance.Audio` |

## 계층 구조

```
[PoolManager] (DontDestroyOnLoad)
└── [PoolContainers]
    ├── [Pool_UI]
    │   └── [PoolContainers]
    │       └── [Pool] PopupPrefab
    ├── [Pool_EFFECT]
    │   └── [PoolContainers]
    │       └── [Pool] ExplosionPrefab
    ├── [Pool_IN_GAME_OBJECT]
    │   └── [PoolContainers]
    │       └── [Pool] EnemyPrefab
    └── [Pool_AUDIO]
        └── [PoolContainers]
            └── [Pool] SoundObject
```

## 사용 예시

### 프로퍼티 접근 (권장)
```csharp
// UI 풀 사용
PoolManager.Instance.UI.RegisterPrefabAsync("UI/Popup", config);
var popup = PoolManager.Instance.UI.Spawn("UI/Popup", parent);
PoolManager.Instance.UI.Despawn(popup);

// Effect 풀 사용
var explosion = PoolManager.Instance.Effect.Spawn("Prefabs/Explosion", position, rotation);
PoolManager.Instance.Effect.DespawnDelayed(explosion, 2f);

// InGameObject 풀 사용
var enemy = PoolManager.Instance.InGameObject.Spawn<EnemyController>("Prefabs/Enemy", position, rotation);
PoolManager.Instance.InGameObject.Despawn(enemy.gameObject);

// Audio 풀 사용
var sound = PoolManager.Instance.Audio.Spawn<SoundObject>("Audio/SoundObject", position, rotation, parent);
PoolManager.Instance.Audio.Despawn(sound.gameObject);
```

### 단축 API
```csharp
// 카테고리 지정 Spawn
var obj = PoolManager.Instance.Spawn(EPoolCategory.EFFECT, "Prefabs/Explosion", position, rotation);

// 카테고리 지정 Spawn (컴포넌트 포함)
var enemy = PoolManager.Instance.Spawn<EnemyController>(EPoolCategory.IN_GAME_OBJECT, "Prefabs/Enemy", position, rotation);

// 카테고리 지정 Despawn
PoolManager.Instance.Despawn(EPoolCategory.EFFECT, obj);

// 지연 Despawn
PoolManager.Instance.DespawnDelayed(EPoolCategory.EFFECT, obj, 2f);
```

### GetPool 메서드
```csharp
// 카테고리로 풀 인스턴스 가져오기
var effectPool = PoolManager.Instance.GetPool(EPoolCategory.EFFECT);
effectPool.Warmup("Prefabs/Explosion", 10);
```

## 메모리 관리

### 전체 풀 정리
```csharp
// 모든 카테고리 LRU 정리
PoolManager.Instance.ShrinkAll();

// 모든 카테고리 완전 정리
PoolManager.Instance.ClearAll();
```

### 카테고리별 정리
```csharp
// 특정 카테고리 LRU 정리
PoolManager.Instance.ShrinkCategory(EPoolCategory.IN_GAME_OBJECT);

// 특정 카테고리 완전 정리
PoolManager.Instance.ClearCategory(EPoolCategory.IN_GAME_OBJECT);
```

### 통계 조회
```csharp
var stats = PoolManager.Instance.GetStats();

// 카테고리별 통계
Debug.Log($"UI: Active={stats.uiStats.active}, Pooled={stats.uiStats.pooled}, Total={stats.uiStats.total}");
Debug.Log($"Effect: Active={stats.effectStats.active}, Pooled={stats.effectStats.pooled}");
Debug.Log($"InGame: Active={stats.inGameObjectStats.active}, Pooled={stats.inGameObjectStats.pooled}");
Debug.Log($"Audio: Active={stats.audioStats.active}, Pooled={stats.audioStats.pooled}");

// 전체 통계
var total = stats.TotalStats;
Debug.Log($"Total: Active={total.active}, Pooled={total.pooled}, Total={total.total}");
```

## API 레퍼런스

### 프로퍼티
| 프로퍼티 | 반환 타입 | 설명 |
|---------|----------|------|
| `UI` | `GameObjectPool` | UI 카테고리 풀 |
| `Effect` | `GameObjectPool` | Effect 카테고리 풀 |
| `InGameObject` | `GameObjectPool` | InGameObject 카테고리 풀 |
| `Audio` | `GameObjectPool` | Audio 카테고리 풀 |

### 메서드
| 메서드 | 설명 |
|--------|------|
| `GetPool(category)` | 카테고리로 풀 인스턴스 가져오기 |
| `Spawn(category, key)` | 단축 Spawn (기본 위치) |
| `Spawn(category, key, pos, rot)` | 단축 Spawn (지정 위치) |
| `Spawn<T>(category, key, pos, rot, parent)` | 단축 Spawn (컴포넌트 포함) |
| `Despawn(category, obj)` | 단축 Despawn |
| `DespawnDelayed(category, obj, delay)` | 단축 지연 Despawn |
| `ShrinkAll()` | 모든 카테고리 LRU 정리 |
| `ClearAll()` | 모든 카테고리 완전 정리 |
| `ShrinkCategory(category)` | 특정 카테고리 LRU 정리 |
| `ClearCategory(category)` | 특정 카테고리 완전 정리 |
| `GetStats()` | 풀 통계 조회 |

## 데이터 구조

### EPoolCategory
```csharp
public enum EPoolCategory
{
    UI,              // UI 관련 오브젝트
    EFFECT,          // 이펙트
    IN_GAME_OBJECT,  // 인게임 오브젝트
    AUDIO            // 오디오 관련
}
```

### CategoryPoolSettings
```csharp
[Serializable]
public class CategoryPoolSettings
{
    public bool autoShrink = true;
    public float shrinkCheckInterval = 10f;
    public List<GameObjectPoolConfig> preloadPools = new();
}
```

### PoolManagerStats
```csharp
public class PoolManagerStats
{
    public (int active, int pooled, int total) uiStats;
    public (int active, int pooled, int total) effectStats;
    public (int active, int pooled, int total) inGameObjectStats;
    public (int active, int pooled, int total) audioStats;
    public (int active, int pooled, int total) TotalStats { get; }
}
```

## Inspector 설정

```csharp
[Header("카테고리별 풀 설정")]
[SerializeField] private CategoryPoolSettings uiSettings;
[SerializeField] private CategoryPoolSettings effectSettings;
[SerializeField] private CategoryPoolSettings inGameObjectSettings;
[SerializeField] private CategoryPoolSettings audioSettings;
```

## 초기화 순서

```
1. AssetLoader 초기화
2. PoolManager 초기화
   ├── UI 풀 생성 및 초기화
   ├── Effect 풀 생성 및 초기화
   ├── InGameObject 풀 생성 및 초기화
   └── Audio 풀 생성 및 초기화
3. AudioManager 초기화 (Audio 풀 사용)
```

## 마이그레이션 가이드

### 기존 코드 (GameObjectPool 직접 사용)
```csharp
// Before
GameObjectPool.Instance.RegisterPrefab("Prefabs/Enemy", config);
var enemy = GameObjectPool.Instance.Spawn("Prefabs/Enemy", position, rotation);
GameObjectPool.Instance.Despawn(enemy);
```

### 새 코드 (PoolManager 사용)
```csharp
// After - 적절한 카테고리 선택
PoolManager.Instance.InGameObject.RegisterPrefab("Prefabs/Enemy", config);
var enemy = PoolManager.Instance.InGameObject.Spawn("Prefabs/Enemy", position, rotation);
PoolManager.Instance.InGameObject.Despawn(enemy);
```

## 주의사항

- `PoolManager`는 싱글톤으로 씬 전환 시에도 유지됨 (`DontDestroyOnLoad`)
- `AssetLoader`가 먼저 초기화되어 있어야 함
- 각 카테고리 풀은 독립적인 메모리 관리 수행
- 씬 전환 시 필요에 따라 `ClearCategory()`로 특정 카테고리만 정리 가능
