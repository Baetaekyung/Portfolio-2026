using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour
    where T : MonoBehaviour
{
    private static T _instance;

    // 싱글톤 인스턴스 접근자
    public static T Instance
    {
        get
        {
            // 인스턴스가 없으면 생성
            if (_instance == null)
            {
                // 씬에서 기존 인스턴스 검색
                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

                // 없다면 새로 생성
                if (_instance == null)
                {
                    var singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        // 이미 인스턴스가 존재하는지 확인
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 현재 인스턴스를 _instance로 설정
        _instance = this as T;

        /*
        SingletonFlag 속성 확인 및 DontDestroyOnLoad 적용
        DONT_DESTROY 플래그가 설정되어 있으면 씬 전환 시에도 유지
        */
        var attributes = typeof(T).GetCustomAttributes(typeof(SingletonFlag), true);
        if (attributes.Length > 0)
        {
            var flag = (SingletonFlag)attributes[0];
            if (flag.Flag == ESingletonFlag.DONT_DESTROY)
            {
                /* 직접적으로 DontDestory 선언을 하는 것이 더 성능에는 
                   좋지만 프로젝트의 일관성 및 디버깅을 위해 이렇게 작성한다 
                   DontDestoryMonitor 참조 */
                _instance.gameObject.AddComponent<DontDestroyBehaviour>();
            }
        }

        // 초기화 메서드 호출
        OnCreated();
    }

    // 싱글톤이 처음 생성될 때 호출되는 메서드
    protected virtual void OnCreated()
    {
    }
}
