using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

public interface ILocalData
{
    void SetAsDefaultData();
    ILocalData GetDefualtData();
}

public sealed class GameOption : ILocalData
{
    public bool BgmOn { get; private set; } = true;
    public bool SfxOn { get; private set; } = true;
    public bool UiSfxOn { get; private set; } = true;

    public float BgmVolume { get; private set; } = 50f;
    public float SfxVolume { get; private set; } = 50f;
    public float UiSfxVolume { get; private set; } = 50f;

    public bool PostProcessingOn { get; private set; } = false;
    public bool UseHighQuality { get; private set; } = false;

    public int FramePerSecond { get; private set; } = 30;

    public void SetAsDefaultData()
    {
        BgmOn = true;
        SfxOn = true;
        UiSfxOn = true;

        BgmVolume = 50f;
        SfxVolume = 50f;
        UiSfxVolume = 50f;

        PostProcessingOn = false;
        UseHighQuality = false;

        FramePerSecond = 30;
    }
    
    public ILocalData GetDefualtData()
    {
        SetAsDefaultData();

        return this;
    }
}

[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public partial class LocalDataManager : Singleton<LocalDataManager>
{
    // ILocalData 캐시 저장소
    private Dictionary<string, ILocalData> _localDataCache = new Dictionary<string, ILocalData>();

    public void Initialize()
    {
        
    }

    public void SetLocalData<T>(string key, T genericData) where T : struct
    {
        if (genericData is int iData)
        {
            PlayerPrefs.SetInt(key, iData);
        }
        else if (genericData is bool bData)
        {
            PlayerPrefs.SetInt(key, bData ? 1 : 0);
        }
        else if (genericData is float fData)
        {
            PlayerPrefs.SetFloat(key, fData);
        }
        else
        {
            Log.WriteError($"저장할 수 없는 로컬 데이터 타입. Key: {key}");
        }
    }

    public T GetLocalData<T>(string key)
    {
        // 타입별로 PlayerPrefs에서 데이터 로드
        if (typeof(T) == typeof(int))
        {
            return (T)(object)PlayerPrefs.GetInt(key, 0);
        }
        else if (typeof(T) == typeof(bool))
        {
            return (T)(object)(PlayerPrefs.GetInt(key, 0) == 1);
        }
        else if (typeof(T) == typeof(float))
        {
            return (T)(object)PlayerPrefs.GetFloat(key, 0f);
        }
        else if (typeof(ILocalData).IsAssignableFrom(typeof(T)))
        {
            // 캐시에 있으면 캐시에서 반환
            if (_localDataCache.TryGetValue(key, out var cached))
            {
                return (T)cached;
            }

            var data = PlayerPrefs.GetString(key);
            T result;

            // 저장된 데이터가 없으면 기본 데이터 반환
            if (string.IsNullOrEmpty(data))
            {
                var defaultInstance = System.Activator.CreateInstance<T>();
                result = (T)((ILocalData)defaultInstance).GetDefualtData();
            }
            else
            {
                result = JsonConvert.DeserializeObject<T>(data);
            }

            // 캐시에 저장
            _localDataCache[key] = (ILocalData)result;
            return result;
        }
        else
        {
            Log.WriteError($"불러올 수 없는 로컬 데이터 타입. Key: {key}");
            return default;
        }
    }

    public void SetLocalCustomData<T>(string key, T customData) where T : class, ILocalData
    {
        // JSON으로 직렬화하여 저장
        var json = JsonConvert.SerializeObject(customData);
        PlayerPrefs.SetString(key, json);

        // 캐시 업데이트
        _localDataCache[key] = customData;
    }

    /// <summary>
    /// 특정 키의 캐시를 제거합니다.
    /// </summary>
    public void ClearCache(string key)
    {
        _localDataCache.Remove(key);
    }

    /// <summary>
    /// 모든 캐시를 제거합니다.
    /// </summary>
    public void ClearAllCache()
    {
        _localDataCache.Clear();
    }
}
