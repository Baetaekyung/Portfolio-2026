using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public partial class AssetLoader : Singleton<AssetLoader>
{
    // 캐싱된 에셋 저장소
    private readonly Dictionary<string, object> _cached = new();

    // 로드 핸들 저장소 (메모리 해제용)
    private readonly Dictionary<string, AsyncOperationHandle> _handles = new();

    // 씬 핸들 저장소 (씬 언로드 시 해제)
    private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _sceneHandles = new();

    // 인스턴스 핸들 저장소 (인스턴스별 해제용)
    private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instanceHandles = new();

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 에셋을 동기적으로 로드합니다.
    /// 캐시된 에셋이 있으면 캐시에서 반환합니다.
    /// </summary>
    public T Load<T>(string key) where T : class
    {
        // 캐시 확인
        if (_cached.TryGetValue(key, out var cachedAsset))
        {
            return cachedAsset as T;
        }

        // Addressables 동기 로드
        var handle = Addressables.LoadAssetAsync<T>(key);
        var asset = handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _cached[key] = asset;
            _handles[key] = handle;
            return asset;
        }

        Debug.LogError($"[AssetLoader] 에셋 로드 실패: {key}");
        return null;
    }

    /// <summary>
    /// 에셋을 비동기적으로 로드합니다.
    /// 로드 완료 시 콜백을 호출합니다.
    /// </summary>
    public void LoadAsync<T>(string key, Action<T> onComplete) where T : class
    {
        // 캐시 확인
        if (_cached.TryGetValue(key, out var cachedAsset))
        {
            onComplete?.Invoke(cachedAsset as T);
            return;
        }

        // Addressables 비동기 로드
        var handle = Addressables.LoadAssetAsync<T>(key);
        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _cached[key] = op.Result;
                _handles[key] = op;
                onComplete?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"[AssetLoader] 에셋 로드 실패: {key}");
                onComplete?.Invoke(null);
            }
        };
    }

    /// <summary>
    /// 특정 에셋을 메모리에서 해제합니다.
    /// </summary>
    public void Release(string key)
    {
        if (_handles.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            _handles.Remove(key);
        }

        _cached.Remove(key);
    }

    /// <summary>
    /// 모든 캐시된 에셋을 메모리에서 해제합니다.
    /// </summary>
    public void ReleaseAll()
    {
        // 에셋 핸들 정리
        foreach (var handle in _handles.Values)
        {
            Addressables.Release(handle);
        }

        // 씬 핸들 정리
        foreach (var sceneHandle in _sceneHandles.Values)
        {
            Addressables.UnloadSceneAsync(sceneHandle);
        }

        // 인스턴스 핸들 정리
        ReleaseAllInstances();

        _handles.Clear();
        _cached.Clear();
        _sceneHandles.Clear();
    }

    /// <summary>
    /// 에셋이 캐시되어 있는지 확인합니다.
    /// </summary>
    public bool IsCached(string key)
    {
        return _cached.ContainsKey(key);
    }

    #region Instantiate

    /// <summary>
    /// GameObject를 동기적으로 인스턴스화합니다.
    /// </summary>
    public GameObject Instantiate(string key)
    {
        return Instantiate(key, Vector3.zero, Quaternion.identity, null);
    }

    /// <summary>
    /// GameObject를 동기적으로 인스턴스화합니다. (위치/회전 지정)
    /// </summary>
    public GameObject Instantiate(string key, Vector3 position, Quaternion rotation)
    {
        return Instantiate(key, position, rotation, null);
    }

    /// <summary>
    /// GameObject를 동기적으로 인스턴스화합니다. (부모 지정)
    /// </summary>
    public GameObject Instantiate(string key, Transform parent)
    {
        return Instantiate(key, Vector3.zero, Quaternion.identity, parent);
    }

    /// <summary>
    /// GameObject를 동기적으로 인스턴스화합니다. (전체 옵션)
    /// </summary>
    public GameObject Instantiate(string key, Vector3 position, Quaternion rotation, Transform parent)
    {
        var handle = Addressables.InstantiateAsync(key, position, rotation, parent);
        var instance = handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _instanceHandles[instance] = handle;
            return instance;
        }

        Debug.LogError($"[AssetLoader] 인스턴스 생성 실패: {key}");
        return null;
    }

    /// <summary>
    /// GameObject를 비동기적으로 인스턴스화합니다.
    /// </summary>
    public void InstantiateAsync(string key, Action<GameObject> onComplete)
    {
        InstantiateAsync(key, Vector3.zero, Quaternion.identity, null, onComplete);
    }

    /// <summary>
    /// GameObject를 비동기적으로 인스턴스화합니다. (위치/회전 지정)
    /// </summary>
    public void InstantiateAsync(string key, Vector3 position, Quaternion rotation, Action<GameObject> onComplete)
    {
        InstantiateAsync(key, position, rotation, null, onComplete);
    }

    /// <summary>
    /// GameObject를 비동기적으로 인스턴스화합니다. (부모 지정)
    /// </summary>
    public void InstantiateAsync(string key, Transform parent, Action<GameObject> onComplete)
    {
        InstantiateAsync(key, Vector3.zero, Quaternion.identity, parent, onComplete);
    }

    /// <summary>
    /// GameObject를 비동기적으로 인스턴스화합니다. (전체 옵션)
    /// </summary>
    public void InstantiateAsync(string key, Vector3 position, Quaternion rotation, Transform parent, Action<GameObject> onComplete)
    {
        var handle = Addressables.InstantiateAsync(key, position, rotation, parent);
        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _instanceHandles[op.Result] = op;
                onComplete?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"[AssetLoader] 인스턴스 생성 실패: {key}");
                onComplete?.Invoke(null);
            }
        };
    }

    /// <summary>
    /// 컴포넌트와 함께 GameObject를 동기적으로 인스턴스화합니다.
    /// </summary>
    public T Instantiate<T>(string key, Vector3 position = default, Quaternion rotation = default, Transform parent = null) where T : Component
    {
        var instance = Instantiate(key, position, rotation, parent);
        return instance != null ? instance.GetComponent<T>() : null;
    }

    /// <summary>
    /// 컴포넌트와 함께 GameObject를 비동기적으로 인스턴스화합니다.
    /// </summary>
    public void InstantiateAsync<T>(string key, Vector3 position = default, Quaternion rotation = default, Transform parent = null, Action<T> onComplete = null) where T : Component
    {
        InstantiateAsync(key, position, rotation, parent, instance =>
        {
            onComplete?.Invoke(instance != null ? instance.GetComponent<T>() : null);
        });
    }

    /// <summary>
    /// 인스턴스를 해제합니다.
    /// </summary>
    public void ReleaseInstance(GameObject instance)
    {
        if (instance == null)
            return;

        if (_instanceHandles.TryGetValue(instance, out var handle))
        {
            Addressables.ReleaseInstance(handle);
            _instanceHandles.Remove(instance);
        }
        else
        {
            // 핸들이 없으면 일반 Destroy
            Destroy(instance);
        }
    }

    /// <summary>
    /// 모든 인스턴스를 해제합니다.
    /// </summary>
    public void ReleaseAllInstances()
    {
        foreach (var kvp in _instanceHandles)
        {
            if (kvp.Key != null)
            {
                Addressables.ReleaseInstance(kvp.Value);
            }
        }

        _instanceHandles.Clear();
    }

    #endregion

    private void OnDestroy()
    {
        ReleaseAll();
    }
}
