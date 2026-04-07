using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GameObjectPool 기반 오디오 관리 시스템
SFX와 BGM을 효율적으로 관리
*/
[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public class AudioManager : Singleton<AudioManager>
{
    [Header("볼륨 설정")]
    [SerializeField, Range(0, 1)] private float masterVolume = 1f;
    [SerializeField, Range(0, 1)] private float bgmVolume = 1f;
    [SerializeField, Range(0, 1)] private float sfxVolume = 1f;

    [Header("풀 설정")]
    [SerializeField] private string soundObjectKey = "Audio/SoundObject";
    [SerializeField] private int initialPoolSize = 10;

    // BGM 전용 SoundObject
    private SoundObject _bgmSource;
    private string _currentBgmKey;
    private Coroutine _bgmFadeCoroutine;

    // 활성 SFX 추적
    private readonly Dictionary<int, SoundObject> _activeSfx = new();

    // 음소거 상태
    private bool _isMuted;

    public float MasterVolume => masterVolume;
    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;
    public bool IsMuted => _isMuted;

    protected override void OnCreated()
    {
        base.OnCreated();

        // SoundObject 풀 등록 (Audio 카테고리)
        PoolManager.Instance.Audio.RegisterPrefabAsync(soundObjectKey, new GameObjectPoolConfig
        {
            AddressableKey = soundObjectKey,
            InitialSize = initialPoolSize,
            MaxSize = 50
        });

        // BGM 전용 오브젝트 생성
        CreateBgmSource();
    }

    // BGM 전용 AudioSource 생성
    private void CreateBgmSource()
    {
        var bgmObj = new GameObject("[BGM Source]");
        bgmObj.transform.SetParent(transform);

        var audioSource = bgmObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;

        _bgmSource = bgmObj.AddComponent<SoundObject>();
    }

    #region BGM

    /// <summary>
    /// BGM 재생 (기존 BGM 자동 정지)
    /// </summary>
    public void PlayBGM(string key, float volume = 1f, float fadeIn = 0f)
    {
        // 동일한 BGM이면 무시
        if (_currentBgmKey == key && _bgmSource.IsPlaying)
            return;

        // 기존 BGM 정지
        StopBgmFade();

        // AudioClip 로드
        AssetLoader.Instance.LoadAsync<AudioClip>(key, clip =>
        {
            if (clip == null)
            {
                Debug.LogError($"[AudioManager] BGM 로드 실패: {key}");
                return;
            }

            _currentBgmKey = key;
            var finalVolume = GetBgmVolume(volume);

            if (fadeIn > 0)
            {
                _bgmSource.Play(clip, 0f, true);
                _bgmFadeCoroutine = StartCoroutine(FadeBgmCoroutine(0f, finalVolume, fadeIn));
            }
            else
            {
                _bgmSource.Play(clip, finalVolume, true);
            }
        });
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM(float fadeOut = 0f)
    {
        StopBgmFade();

        if (!_bgmSource.IsPlaying)
            return;

        if (fadeOut > 0)
        {
            _bgmFadeCoroutine = StartCoroutine(FadeBgmCoroutine(_bgmSource.AudioSource.volume, 0f, fadeOut, () =>
            {
                _bgmSource.Stop();
                _currentBgmKey = null;
            }));
        }
        else
        {
            _bgmSource.Stop();
            _currentBgmKey = null;
        }
    }

    /// <summary>
    /// BGM 일시정지
    /// </summary>
    public void PauseBGM()
    {
        if (_bgmSource.AudioSource != null)
        {
            _bgmSource.AudioSource.Pause();
        }
    }

    /// <summary>
    /// BGM 재개
    /// </summary>
    public void ResumeBGM()
    {
        if (_bgmSource.AudioSource != null)
        {
            _bgmSource.AudioSource.UnPause();
        }
    }

    /// <summary>
    /// BGM 볼륨 설정
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateBgmVolume();
    }

    // BGM 페이드 중지
    private void StopBgmFade()
    {
        if (_bgmFadeCoroutine != null)
        {
            StopCoroutine(_bgmFadeCoroutine);
            _bgmFadeCoroutine = null;
        }
    }

    // BGM 페이드 코루틴
    private IEnumerator FadeBgmCoroutine(float from, float to, float duration, Action onComplete = null)
    {
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _bgmSource.SetVolume(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        _bgmSource.SetVolume(to);
        _bgmFadeCoroutine = null;
        onComplete?.Invoke();
    }

    // BGM 볼륨 업데이트
    private void UpdateBgmVolume()
    {
        if (_bgmSource != null && _bgmSource.IsPlaying)
        {
            _bgmSource.SetVolume(GetBgmVolume(1f));
        }
    }

    // 최종 BGM 볼륨 계산
    private float GetBgmVolume(float baseVolume)
    {
        return _isMuted ? 0f : baseVolume * bgmVolume * masterVolume;
    }

    #endregion

    #region SFX

    /// <summary>
    /// SFX OneShot 재생
    /// </summary>
    public void PlaySFX(string key, float volume = 1f)
    {
        PlaySfxInternal(key, volume, false);
    }

    /// <summary>
    /// SFX Loop 재생 (핸들 반환)
    /// </summary>
    public int PlaySFXLoop(string key, float volume = 1f)
    {
        return PlaySfxInternal(key, volume, true);
    }

    /// <summary>
    /// 특정 SFX 정지
    /// </summary>
    public void StopSFX(int handleId)
    {
        if (_activeSfx.TryGetValue(handleId, out var soundObject))
        {
            _activeSfx.Remove(handleId);
            soundObject.Stop();
            PoolManager.Instance.Audio.Despawn(soundObject.gameObject);
        }
    }

    /// <summary>
    /// 특정 SFX 페이드 아웃 후 정지
    /// </summary>
    public void StopSFXWithFade(int handleId, float fadeOut)
    {
        if (_activeSfx.TryGetValue(handleId, out var soundObject))
        {
            _activeSfx.Remove(handleId);
            soundObject.StopWithFade(fadeOut, () =>
            {
                PoolManager.Instance.Audio.Despawn(soundObject.gameObject);
            });
        }
    }

    /// <summary>
    /// 모든 SFX 정지
    /// </summary>
    public void StopAllSFX()
    {
        foreach (var kvp in _activeSfx)
        {
            var soundObject = kvp.Value;
            if (soundObject != null)
            {
                soundObject.Stop();
                PoolManager.Instance.Audio.Despawn(soundObject.gameObject);
            }
        }

        _activeSfx.Clear();
    }

    /// <summary>
    /// SFX 볼륨 설정
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAllSfxVolume();
    }

    // SFX 재생 내부 구현
    private int PlaySfxInternal(string key, float volume, bool loop)
    {
        // SoundObject 스폰
        var soundObject = PoolManager.Instance.Audio.Spawn<SoundObject>(soundObjectKey, transform.position, Quaternion.identity, transform);

        if (soundObject == null)
        {
            Debug.LogError($"[AudioManager] SoundObject 스폰 실패: {key}");
            return -1;
        }

        var handleId = soundObject.HandleId;

        // AudioClip 로드 및 재생
        AssetLoader.Instance.LoadAsync<AudioClip>(key, clip =>
        {
            if (clip == null)
            {
                Debug.LogError($"[AudioManager] SFX 로드 실패: {key}");
                PoolManager.Instance.Audio.Despawn(soundObject.gameObject);
                return;
            }

            var finalVolume = GetSfxVolume(volume);
            soundObject.Play(clip, finalVolume, loop);

            // Loop인 경우 활성 목록에 추가
            if (loop)
            {
                _activeSfx[handleId] = soundObject;
            }
        });

        return handleId;
    }

    // 모든 활성 SFX 볼륨 업데이트
    private void UpdateAllSfxVolume()
    {
        foreach (var kvp in _activeSfx)
        {
            kvp.Value?.SetVolume(GetSfxVolume(1f));
        }
    }

    // 최종 SFX 볼륨 계산
    private float GetSfxVolume(float baseVolume)
    {
        return _isMuted ? 0f : baseVolume * sfxVolume * masterVolume;
    }

    #endregion

    #region 전역 설정

    /// <summary>
    /// 마스터 볼륨 설정
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateBgmVolume();
        UpdateAllSfxVolume();
    }

    /// <summary>
    /// 음소거 설정
    /// </summary>
    public void Mute(bool mute)
    {
        _isMuted = mute;
        UpdateBgmVolume();
        UpdateAllSfxVolume();
    }

    /// <summary>
    /// 모든 사운드 정지
    /// </summary>
    public void StopAll()
    {
        StopBGM();
        StopAllSFX();
    }

    #endregion

    private void OnDestroy()
    {
        StopAll();
    }
}
