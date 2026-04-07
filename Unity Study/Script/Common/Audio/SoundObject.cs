using System;
using System.Collections;
using UnityEngine;

/*
풀링 가능한 사운드 오브젝트
AudioSource를 래핑하여 재생 완료 시 자동 반환
*/
[RequireComponent(typeof(AudioSource))]
public class SoundObject : MonoBehaviour, GameObjectPool.IPoolable
{
    private AudioSource _audioSource;
    private Coroutine _autoReturnCoroutine;
    private bool _isLoop;

    // 고유 ID (Loop SFX 정지용)
    public int HandleId { get; private set; }

    public AudioSource AudioSource => _audioSource;
    public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;
    public bool IsLoop => _isLoop;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    #region IPoolable

    public void OnSpawn()
    {
        // 초기화
        HandleId = GetInstanceID() + Time.frameCount;
    }

    public void OnDespawn()
    {
        // 정리
        Stop();
    }

    #endregion

    /// <summary>
    /// 사운드 재생
    /// </summary>
    public void Play(AudioClip clip, float volume = 1f, bool loop = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundObject] AudioClip이 null입니다.");
            return;
        }

        _isLoop = loop;
        _audioSource.clip = clip;
        _audioSource.volume = volume;
        _audioSource.loop = loop;
        _audioSource.Play();

        // OneShot인 경우 재생 완료 후 자동 반환
        if (!loop)
        {
            StartAutoReturn(clip.length);
        }
    }

    /// <summary>
    /// 볼륨 변경
    /// </summary>
    public void SetVolume(float volume)
    {
        if (_audioSource != null)
        {
            _audioSource.volume = volume;
        }
    }

    /// <summary>
    /// 사운드 정지
    /// </summary>
    public void Stop()
    {
        StopAutoReturn();

        if (_audioSource != null)
        {
            _audioSource.Stop();
            _audioSource.clip = null;
        }
    }

    /// <summary>
    /// 페이드 아웃 후 정지
    /// </summary>
    public void StopWithFade(float duration, Action onComplete = null)
    {
        StopAutoReturn();
        StartCoroutine(FadeOutCoroutine(duration, onComplete));
    }

    // 자동 반환 시작
    private void StartAutoReturn(float delay)
    {
        StopAutoReturn();
        _autoReturnCoroutine = StartCoroutine(AutoReturnCoroutine(delay));
    }

    // 자동 반환 취소
    private void StopAutoReturn()
    {
        if (_autoReturnCoroutine != null)
        {
            StopCoroutine(_autoReturnCoroutine);
            _autoReturnCoroutine = null;
        }
    }

    // 재생 완료 후 자동 반환 코루틴
    private IEnumerator AutoReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        _autoReturnCoroutine = null;

        // 풀에 반환 (Audio 카테고리)
        PoolManager.Instance.Audio.Despawn(gameObject);
    }

    // 페이드 아웃 코루틴
    private IEnumerator FadeOutCoroutine(float duration, Action onComplete)
    {
        var startVolume = _audioSource.volume;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        _audioSource.volume = 0f;
        Stop();
        onComplete?.Invoke();
    }
}
