# ObjectPool

## 개요
제네릭 오브젝트 풀링 시스템입니다.
**LRU(Least Recently Used) 방식**으로 오브젝트를 관리하여 메모리를 효율적으로 사용합니다.

## 클래스 정보
- **위치**: `Assets/Script/Common/Pool/ObjectPool.cs`
- **제네릭**: `ObjectPool<T> where T : class`

## 핵심 개념

### LRU (Least Recently Used)
```
1. 오브젝트 사용 시 → 사용 시간 기록 (가장 최근)
2. 풀 크기 초과 시 → 가장 오래 사용 안 된 오브젝트부터 삭제
3. 자동 정리 → 일정 시간마다 미사용 오브젝트 정리
```

### 풀 상태
| 상태 | 설명 |
|------|------|
| `Active` | 현재 사용 중인 오브젝트 |
| `Pooled` | 풀에서 대기 중인 오브젝트 |
| `Disposed` | 정리된 오브젝트 |

## 데이터 구조

### PooledObject<T>
```csharp
public class PooledObject<T> where T : class
{
    public T Object;              // 실제 오브젝트
    public float LastUsedTime;    // 마지막 사용 시간
    public bool IsActive;         // 활성 상태
}
```

### PoolConfig
```csharp
public class PoolConfig
{
    public int InitialSize;       // 초기 생성 개수
    public int MaxSize;           // 최대 크기
    public float AutoShrinkTime;  // 자동 정리 주기 (초)
    public float ExpireTime;      // 미사용 만료 시간 (초)
}
```

## 사용 예시

### 기본 사용
```csharp
// 풀 생성
var pool = new ObjectPool<MyClass>(
    createFunc: () => new MyClass(),
    onGet: obj => obj.Reset(),
    onRelease: obj => obj.Clear(),
    onDestroy: obj => obj.Dispose()
);

// 오브젝트 가져오기
var obj = pool.Get();

// 오브젝트 반환
pool.Release(obj);
```

### 설정과 함께 사용
```csharp
var config = new PoolConfig
{
    InitialSize = 10,
    MaxSize = 100,
    AutoShrinkTime = 30f,
    ExpireTime = 60f
};

var pool = new ObjectPool<Bullet>(
    createFunc: () => new Bullet(),
    config: config
);
```

### 자동 반환 (IDisposable 패턴)
```csharp
using (var handle = pool.GetAutoRelease())
{
    var obj = handle.Object;
    // 사용...
} // 자동으로 Release 호출됨
```

## API 레퍼런스

### 생성자
```csharp
public ObjectPool(
    Func<T> createFunc,                    // 생성 함수 (필수)
    Action<T> onGet = null,                // Get 시 콜백
    Action<T> onRelease = null,            // Release 시 콜백
    Action<T> onDestroy = null,            // Destroy 시 콜백
    PoolConfig config = null               // 설정
)
```

### 주요 메서드
| 메서드 | 설명 |
|--------|------|
| `Get()` | 풀에서 오브젝트 가져오기 |
| `Release(T obj)` | 오브젝트를 풀에 반환 |
| `GetAutoRelease()` | 자동 반환 핸들 가져오기 |
| `Shrink()` | LRU 기반으로 풀 정리 |
| `Clear()` | 모든 오브젝트 정리 |
| `Warmup(int count)` | 미리 오브젝트 생성 |

### 프로퍼티
| 프로퍼티 | 설명 |
|----------|------|
| `ActiveCount` | 현재 사용 중인 오브젝트 수 |
| `PooledCount` | 풀에서 대기 중인 오브젝트 수 |
| `TotalCount` | 전체 오브젝트 수 |

## LRU 정리 로직

```csharp
// Shrink 호출 시:
1. 풀에서 대기 중인 오브젝트 중
2. ExpireTime보다 오래된 것들을 찾아서
3. 가장 오래된 것부터 삭제
4. MaxSize 이하가 될 때까지 반복
```

## 주의사항

- `Get()`으로 가져온 오브젝트는 반드시 `Release()`로 반환
- `onDestroy` 콜백에서 리소스 정리 필수
- 멀티스레드 환경에서는 별도 동기화 필요
- Unity 환경에서 MonoBehaviour 사용 시 GameObjectPool 권장
