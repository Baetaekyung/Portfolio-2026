using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// SpriteAtlas를 관리하고 이름으로 스프라이트를 가져오는 매니저
/// </summary>
[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public class AtlasManager : Singleton<AtlasManager>
{
    public const string UI_ATLAS = "ATLAS_UI";
    public const string ILLUST_ATLAS = "ATLAS_Illust";

    // 등록된 아틀라스 저장소
    private readonly Dictionary<string, SpriteAtlas> _atlasDict = new();

    // 스프라이트 캐시 (atlasKey_spriteName -> Sprite)
    private readonly Dictionary<string, Sprite> _spriteCache = new();

    [SerializeField] private SpriteAtlas[] atlases;

    protected override void OnCreated()
    {
        base.OnCreated();

        foreach (var preRegisterAtlas in atlases)
        {
            RegisterAtlas(preRegisterAtlas);
        }
    }

    /// <summary>
    /// 아틀라스를 키와 함께 등록
    /// </summary>
    public void RegisterAtlas(string key, SpriteAtlas atlas)
    {
        if (string.IsNullOrEmpty(key) || atlas == null)
        {
            Log.WriteLog("[AtlasManager] 잘못된 키 또는 아틀라스입니다.");
            return;
        }

        if (_atlasDict.ContainsKey(key))
        {
            Log.WriteLog($"[AtlasManager] 이미 등록된 아틀라스 키입니다: {key}");
            return;
        }

        Log.WriteLog($"[AtlasManager] {key} 아틀라스 등록 완료.");
        _atlasDict[key] = atlas;
    }

    /// <summary>
    /// 아틀라스를 이름으로 등록 (아틀라스 이름을 키로 사용)
    /// </summary>
    public void RegisterAtlas(SpriteAtlas atlas)
    {
        if (atlas == null)
        {
            Log.WriteWarning("[AtlasManager] 아틀라스가 null입니다.");
            return;
        }
        RegisterAtlas(atlas.name, atlas);
    }

    /// <summary>
    /// 특정 아틀라스에서 스프라이트 가져오기
    /// </summary>
    public Sprite GetSprite(string atlasKey, string spriteName)
    {
        if (string.IsNullOrEmpty(atlasKey) || string.IsNullOrEmpty(spriteName))
            return null;

        // 캐시 확인
        var cacheKey = $"{atlasKey}_{spriteName}";
        if (_spriteCache.TryGetValue(cacheKey, out var cachedSprite))
            return cachedSprite;

        // 아틀라스에서 스프라이트 검색
        if (!_atlasDict.TryGetValue(atlasKey, out var atlas))
        {
            Log.WriteWarning($"[AtlasManager] 등록되지 않은 아틀라스입니다: {atlasKey}");
            return null;
        }

        var sprite = atlas.GetSprite(spriteName);
        if (sprite != null)
        {
            _spriteCache[cacheKey] = sprite;
        }
        else
        {
            Log.WriteWarning($"{spriteName} sprite key is doesn't exist.");
        }

        return sprite;
    }

    /// <summary>
    /// 등록된 모든 아틀라스에서 스프라이트 검색
    /// </summary>
    public Sprite GetSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
            return null;

        // 전체 아틀라스에서 검색
        foreach (var kvp in _atlasDict)
        {
            var sprite = GetSprite(kvp.Key, spriteName);
            if (sprite != null)
                return sprite;
        }

        Log.WriteWarning($"[AtlasManager] 스프라이트를 찾을 수 없습니다: {spriteName}");
        return null;
    }

    /// <summary>
    /// 특정 아틀라스 해제
    /// </summary>
    public void UnregisterAtlas(string key)
    {
        if (!_atlasDict.ContainsKey(key))
            return;

        _atlasDict.Remove(key);

        // 해당 아틀라스의 캐시 제거
        var keysToRemove = new List<string>();
        foreach (var cacheKey in _spriteCache.Keys)
        {
            if (cacheKey.StartsWith($"{key}_"))
                keysToRemove.Add(cacheKey);
        }

        foreach (var cacheKey in keysToRemove)
        {
            _spriteCache.Remove(cacheKey);
        }
    }

    /// <summary>
    /// 모든 아틀라스 및 캐시 해제
    /// </summary>
    public void Clear()
    {
        _atlasDict.Clear();
        _spriteCache.Clear();
    }
}
