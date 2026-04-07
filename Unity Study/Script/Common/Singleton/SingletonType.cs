public abstract class SingletonType<T>
    where T : SingletonType<T>, new()
{
    private static T _instance;

    // 싱글톤 인스턴스 생성 및 반환
    public static T Create()
    {
        if (_instance == null)
        {
            _instance = new T();
            _instance.OnCreated();
            Log.WriteLog($"[Singleton] {typeof(T).Name}의 객체가 생성되었다");
        }

        return _instance;
    }

    // 싱글톤 인스턴스 접근자
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                Create();
            }

            return _instance;
        }
    }

    // 생성자를 protected로 선언하여 외부에서 직접 인스턴스화 방지
    protected SingletonType()
    {
    }

    // 싱글톤이 처음 생성될 때 호출되는 메서드
    protected virtual void OnCreated()
    {
    }
}
