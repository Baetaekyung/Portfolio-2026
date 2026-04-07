# SingletonType 구현 목표

## 개요
MonoBehaviour를 상속받지 않는 일반 C# 클래스용 제네릭 싱글톤 패턴을 구현합니다.

## 구현 요구사항

### 1. 기본 구조
- `MonoBehaviour`를 상속받지 **않습니다**
- 제네릭 형식으로 구현되어야 합니다: `SingletonType<T> where T : SingletonType<T>, new()`
- `abstract` 클래스로 선언하여 직접 인스턴스화를 방지합니다
- `new()` 제약 조건을 통해 인스턴스 생성을 가능하게 합니다

### 2. 핵심 멤버

#### Create 메서드
```csharp
public static T Create()
```
- 싱글톤 인스턴스를 생성하고 반환합니다
- `_instance`가 `null`인 경우에만 새로 생성합니다
- 생성 후 `OnCreated()` 메서드를 호출합니다
- 이미 인스턴스가 존재하면 기존 인스턴스를 반환합니다

#### Instance 프로퍼티
```csharp
public static T Instance { get; }
```
- `_instance` 필드를 반환합니다
- `_instance`가 `null`인 경우 자동으로 `Create()`를 호출합니다
- 편리한 접근을 위한 프로퍼티입니다

#### _instance 필드
```csharp
private static T _instance;
```
- 싱글톤 인스턴스를 저장하는 정적 필드

#### 생성자
```csharp
protected SingletonType()
```
- `protected`로 선언하여 외부에서 `new` 키워드로 직접 생성하는 것을 방지합니다
- 상속받은 클래스에서는 생성 가능하도록 허용합니다

#### OnCreated 메서드
```csharp
protected virtual void OnCreated()
```
- 싱글톤이 처음 생성될 때 호출됩니다
- 상속받은 클래스에서 오버라이드하여 초기화 로직을 구현할 수 있습니다

### 3. 인스턴스 생성 로직

#### 생성 조건
- `Create()` 메서드 호출 시 `_instance`가 `null`인 경우에만 생성
- `new T()`를 사용하여 새로운 인스턴스 생성

#### 생성 방법
```csharp
public static T Create()
{
    if (_instance == null)
    {
        _instance = new T();
        _instance.OnCreated();
    }

    return _instance;
}
```

#### 중복 방지
- 이미 `_instance`가 존재하는 경우 새로 생성하지 않고 기존 인스턴스를 반환합니다
- 정적 필드를 사용하여 애플리케이션 전체에서 단일 인스턴스만 유지합니다

### 4. Singleton과의 차이점

| 특징 | Singleton<T> | SingletonType<T> |
|------|--------------|------------------|
| 기반 클래스 | MonoBehaviour | 일반 C# 클래스 |
| 생성 방식 | GameObject.AddComponent | new T() |
| Unity 라이프사이클 | 있음 (Awake, Start 등) | 없음 |
| DontDestroyOnLoad | 지원 | 불필요 (씬과 무관) |
| 사용 대상 | Unity 게임 오브젝트 | 순수 데이터/로직 관리 |

### 5. 제네릭 제약 조건

```csharp
where T : SingletonType<T>, new()
```

- `T : SingletonType<T>`: T는 반드시 SingletonType<T>를 상속받아야 합니다 (CRTP 패턴)
- `new()`: T는 매개변수 없는 생성자를 가져야 합니다 (인스턴스 생성을 위해 필요)

## 사용 예시

```csharp
public class DataManager : SingletonType<DataManager>
{
    private Dictionary<string, object> data;

    protected override void OnCreated()
    {
        base.OnCreated();
        // DataManager 초기화 로직
        data = new Dictionary<string, object>();
        LoadInitialData();
    }

    public void SaveData(string key, object value)
    {
        data[key] = value;
    }

    public object GetData(string key)
    {
        return data.ContainsKey(key) ? data[key] : null;
    }

    private void LoadInitialData()
    {
        // 초기 데이터 로드
    }
}

// 다른 스크립트에서 사용 - 방법 1
DataManager.Create().SaveData("score", 100);

// 다른 스크립트에서 사용 - 방법 2
DataManager.Instance.SaveData("score", 100);
var score = DataManager.Instance.GetData("score");
```

## 주의사항

- MonoBehaviour를 상속받지 않기 때문에 Unity의 라이프사이클 메서드를 사용할 수 없습니다
- GameObject에 컴포넌트로 추가할 수 없습니다
- 순수한 C# 클래스이므로 Unity의 Inspector에서 확인할 수 없습니다
- 멀티스레드 환경에서는 안전하지 않을 수 있습니다 (필요시 lock 사용 고려)
- 애플리케이션 종료 시까지 메모리에 유지되므로 메모리 관리에 주의해야 합니다
- Unity가 아닌 순수 C# 환경에서도 사용 가능합니다

## 사용 권장 사항

### SingletonType<T>를 사용해야 하는 경우:
- 게임 데이터 관리 (DataManager, SaveManager 등)
- 설정 관리 (ConfigManager)
- 네트워크 통신 로직
- 순수한 비즈니스 로직
- Unity 컴포넌트가 필요 없는 매니저 클래스

### Singleton<T>를 사용해야 하는 경우:
- Unity 컴포넌트 기능이 필요한 경우 (Coroutine, Physics 등)
- Inspector에서 설정값을 조정해야 하는 경우
- GameObject 기반 기능이 필요한 경우 (AudioSource, Transform 등)
- UI 매니저 등 씬과 연관된 싱글톤
