# AssetLoader

## 개요
Unity Addressables 시스템을 활용한 에셋 로딩 매니저입니다.
싱글톤 패턴으로 구현되어 전역에서 접근 가능합니다.

## 주요 기능

### 1. 동기 로딩
- `T Load<T>(string key)` : 에셋을 동기적으로 로드합니다.
- 캐시된 에셋이 있으면 캐시에서 반환합니다.

### 2. 비동기 로딩
- `void LoadAsync<T>(string key, Action<T> onComplete)` : 콜백 방식 비동기 로드
- `UniTask<T> LoadAsync<T>(string key)` : UniTask 방식 비동기 로드 (선택적)

### 3. 에셋 해제
- `void Release(string key)` : 특정 에셋 해제
- `void ReleaseAll()` : 모든 캐시된 에셋 해제

## 캐싱 정책
- 로드된 에셋은 Dictionary에 캐싱됩니다.
- key를 기준으로 중복 로드를 방지합니다.

## 사용 예시

```csharp
// 동기 로딩
var prefab = AssetLoader.Instance.Load<GameObject>("Prefabs/Player");

// 비동기 로딩 (콜백)
AssetLoader.Instance.LoadAsync<GameObject>("Prefabs/Enemy", (enemy) => {
    Instantiate(enemy);
});

// 에셋 해제
AssetLoader.Instance.Release("Prefabs/Player");

// 모든 에셋 해제
AssetLoader.Instance.ReleaseAll();
```

## 의존성
- Unity Addressables 패키지
