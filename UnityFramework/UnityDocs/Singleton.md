# Singleton 구현 목표

## 개요
MonoBehaviour를 상속받는 제네릭 싱글톤 패턴을 구현합니다.

## 구현 요구사항

### 1. 기본 구조
- `MonoBehaviour`를 상속받아야 합니다
- 제네릭 형식으로 구현되어야 합니다: `Singleton<T> where T : MonoBehaviour`
- `abstract` 클래스로 선언하여 직접 인스턴스화를 방지합니다

### 2. 핵심 멤버

#### Instance 프로퍼티
```csharp
public static T Instance { get; }
```
- `_instance` 필드를 반환합니다
- `_instance`가 `null`인 경우 자동으로 생성해야 합니다

#### _instance 필드
```csharp
private static T _instance;
```
- 싱글톤 인스턴스를 저장하는 정적 필드

#### OnCreated 메서드
```csharp
protected virtual void OnCreated()
```
- 싱글톤이 처음 생성될 때 호출됩니다
- 상속받은 클래스에서 오버라이드하여 초기화 로직을 구현할 수 있습니다

### 3. 인스턴스 생성 로직

#### 생성 조건
- `Instance` 프로퍼티 접근 시 `_instance`가 `null`인 경우에만 생성
- 씬에 이미 해당 타입의 객체가 존재하는지 먼저 확인
  - 존재한다면 그 객체를 `_instance`로 설정
  - 존재하지 않는다면 새로운 GameObject를 생성하고 컴포넌트를 추가

#### 생성 방법
```csharp
// 1. 씬에서 기존 인스턴스 검색
_instance = FindObjectOfType<T>();

// 2. 없다면 새로 생성
if (_instance == null)
{
    GameObject singletonObject = new GameObject(typeof(T).Name);
    _instance = singletonObject.AddComponent<T>();
}
```

#### 중복 방지
- 이미 `_instance`가 존재하는 경우 새로 생성하지 않습니다
- `Awake()`에서 중복 체크를 수행하여 씬에 여러 개의 싱글톤이 존재할 경우 기존 인스턴스 유지하고 새로운 것은 파괴합니다

### 4. SingletonFlag 속성 처리

#### DontDestroyOnLoad 적용
- 클래스에 `[SingletonFlag(ESingletonFlag.DONT_DESTROY)]` 속성이 있는지 확인
- 속성 확인 방법:
  ```csharp
  var attributes = typeof(T).GetCustomAttributes(typeof(SingletonFlag), true);
  if (attributes.Length > 0)
  {
      var flag = (SingletonFlag)attributes[0];
      // flag 값에 따라 DontDestroyOnLoad 적용
  }
  ```
- `DONT_DESTROY` 플래그가 설정되어 있으면 `DontDestroyOnLoad(gameObject)` 호출
- 씬 전환 시에도 싱글톤 인스턴스가 유지됩니다

### 5. Awake 메서드 구조

```csharp
protected virtual void Awake()
{
    // 1. 이미 인스턴스가 존재하는지 확인
    if (_instance != null && _instance != this)
    {
        Destroy(gameObject);
        return;
    }

    // 2. 현재 인스턴스를 _instance로 설정
    _instance = this as T;

    // 3. SingletonFlag 속성 확인 및 DontDestroyOnLoad 적용
    // (DONT_DESTROY 플래그 체크)

    // 4. OnCreated 호출
    OnCreated();
}
```

## 사용 예시

```csharp
[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public class GameManager : Singleton<GameManager>
{
    protected override void OnCreated()
    {
        base.OnCreated();
        // GameManager 초기화 로직
    }

    public void StartGame()
    {
        // 게임 시작 로직
    }
}

// 다른 스크립트에서 사용
GameManager.Instance.StartGame();
```

## 주의사항

- MonoBehaviour를 상속받기 때문에 `new` 키워드로 직접 생성할 수 없습니다
- 반드시 GameObject에 컴포넌트로 추가되어야 합니다
- 멀티스레드 환경에서는 안전하지 않을 수 있습니다 (Unity는 주로 단일 스레드)
- `DONT_DESTROY` 플래그 사용 시 씬 전환 후에도 인스턴스가 유지되므로 메모리 관리에 주의해야 합니다
