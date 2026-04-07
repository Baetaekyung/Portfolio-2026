# AssetLoader

## 개요
Addressables 기반 에셋 로드 및 인스턴스 관리 시스템입니다.
에셋 캐싱, 동기/비동기 로드, GameObject 인스턴스화를 지원합니다.

## 클래스 정보
- **위치**: `Assets/Script/Common/Resource/AssetLoader.cs`
- **상속**: `Singleton<AssetLoader>`
- **의존성**: `UnityEngine.AddressableAssets`

## 핵심 기능

### 에셋 로드 흐름
```
1. 캐시 확인 → 있으면 캐시에서 반환
2. Addressables로 로드
3. 캐시에 저장 및 핸들 관리
4. 에셋 반환
```

### 인스턴스화 흐름
```
1. Addressables.InstantiateAsync로 인스턴스 생성
2. 핸들을 인스턴스별로 저장
3. ReleaseInstance 시 핸들과 함께 해제
```

## 사용 예시

### 에셋 로드

```csharp
// 동기 로드 (캐싱됨)
var clip = AssetLoader.Instance.Load<AudioClip>("Audio/BGM_Main");

// 비동기 로드 (캐싱됨)
AssetLoader.Instance.LoadAsync<Sprite>("UI/Icons/Coin", sprite =>
{
    image.sprite = sprite;
});

// 캐시 확인
if (AssetLoader.Instance.IsCached("Audio/BGM_Main"))
{
    // 이미 로드됨
}

// 특정 에셋 해제
AssetLoader.Instance.Release("Audio/BGM_Main");
```

### GameObject 인스턴스화

```csharp
// 동기 인스턴스화
var enemy = AssetLoader.Instance.Instantiate("Prefabs/Enemy");
var enemy = AssetLoader.Instance.Instantiate("Prefabs/Enemy", position, rotation);
var enemy = AssetLoader.Instance.Instantiate("Prefabs/Enemy", parent);
var enemy = AssetLoader.Instance.Instantiate("Prefabs/Enemy", position, rotation, parent);

// 비동기 인스턴스화
AssetLoader.Instance.InstantiateAsync("Prefabs/Enemy", instance =>
{
    instance.transform.position = spawnPoint;
});

AssetLoader.Instance.InstantiateAsync("Prefabs/Enemy", position, rotation, parent, instance =>
{
    // 생성 완료
});

// 컴포넌트와 함께 인스턴스화
var controller = AssetLoader.Instance.Instantiate<EnemyController>("Prefabs/Enemy", position, rotation);

AssetLoader.Instance.InstantiateAsync<EnemyController>("Prefabs/Enemy", position, rotation, null, controller =>
{
    controller.Initialize(target);
});

// 인스턴스 해제
AssetLoader.Instance.ReleaseInstance(enemy);

// 모든 인스턴스 해제
AssetLoader.Instance.ReleaseAllInstances();
```

### 전체 정리

```csharp
// 모든 캐시 및 인스턴스 해제
AssetLoader.Instance.ReleaseAll();
```

## API 레퍼런스

### 에셋 로드
| 메서드 | 설명 |
|--------|------|
| `Load<T>(key)` | 에셋 동기 로드 (캐싱) |
| `LoadAsync<T>(key, onComplete)` | 에셋 비동기 로드 (캐싱) |
| `IsCached(key)` | 캐시 여부 확인 |
| `Release(key)` | 특정 에셋 해제 |
| `ReleaseAll()` | 모든 에셋 및 인스턴스 해제 |

### 인스턴스화
| 메서드 | 설명 |
|--------|------|
| `Instantiate(key)` | 기본 위치에 인스턴스 생성 |
| `Instantiate(key, pos, rot)` | 지정 위치에 인스턴스 생성 |
| `Instantiate(key, parent)` | 부모 아래에 인스턴스 생성 |
| `Instantiate(key, pos, rot, parent)` | 전체 옵션 인스턴스 생성 |
| `Instantiate<T>(key, pos, rot, parent)` | 컴포넌트와 함께 인스턴스 생성 |
| `InstantiateAsync(key, onComplete)` | 비동기 인스턴스 생성 |
| `InstantiateAsync(key, pos, rot, onComplete)` | 비동기 지정 위치 인스턴스 생성 |
| `InstantiateAsync(key, parent, onComplete)` | 비동기 부모 아래 인스턴스 생성 |
| `InstantiateAsync(key, pos, rot, parent, onComplete)` | 비동기 전체 옵션 인스턴스 생성 |
| `InstantiateAsync<T>(key, pos, rot, parent, onComplete)` | 비동기 컴포넌트 함께 인스턴스 생성 |
| `ReleaseInstance(instance)` | 인스턴스 해제 |
| `ReleaseAllInstances()` | 모든 인스턴스 해제 |

## Load vs Instantiate 비교

| 구분 | Load | Instantiate |
|------|------|-------------|
| 용도 | 에셋 참조 (AudioClip, Sprite 등) | GameObject 생성 |
| 캐싱 | O (동일 키 재사용) | X (매번 새 인스턴스) |
| 해제 | `Release(key)` | `ReleaseInstance(instance)` |
| 반환 | 에셋 자체 | GameObject 인스턴스 |

## 주의사항

- `Load`로 로드한 에셋은 캐싱되어 재사용됨
- `Instantiate`는 매번 새 인스턴스를 생성함
- `ReleaseInstance`로 해제하지 않으면 메모리 누수 발생
- Addressables 키가 잘못되면 로드/생성 실패 로그 출력
- `OnDestroy`에서 자동으로 `ReleaseAll` 호출됨

## PoolManager와의 관계

풀링이 필요한 경우 `AssetLoader.Instantiate` 대신 `PoolManager`를 사용하세요.

```csharp
// 단일 인스턴스 (풀링 불필요)
var ui = AssetLoader.Instance.Instantiate("UI/SettingsPopup");

// 반복 생성/파괴 (풀링 권장)
var bullet = PoolManager.Instance.InGameObject.Spawn("Prefabs/Bullet", position, rotation);
PoolManager.Instance.InGameObject.Despawn(bullet);
```
