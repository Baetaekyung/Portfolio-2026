using UnityEngine;
using DG.Tweening;

public enum TweenAnimationType
{
    NONE,
    BOBBING,    // 위아래로 왔다갔다
    SHAKE,      // 흔들리기
    ROTATE,     // 회전
    SCALE_PULSE,// 크기 펄스
    SWING       // 좌우 흔들림
}

public class TweenBehaviour : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [SerializeField] private TweenAnimationType animationType = TweenAnimationType.NONE;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float strength = 1f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;

    [Header("Bobbing 설정")]
    [SerializeField] private float bobbingHeight = 0.5f;

    [Header("Shake 설정")]
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float shakeRandomness = 90f;

    [Header("Rotate 설정")]
    [SerializeField] private Vector3 rotateAxis = Vector3.forward;

    [Header("Scale Pulse 설정")]
    [SerializeField] private float scaleMultiplier = 1.2f;

    [Header("Swing 설정")]
    [SerializeField] private float swingAngle = 15f;

    private Tween _currentTween;
    private Vector3 _initialPosition;
    private Vector3 _initialRotation;
    private Vector3 _initialScale;

    // 모니터링용 프로퍼티
    public bool IsPlaying => _currentTween != null && _currentTween.IsActive() && _currentTween.IsPlaying();
    public TweenAnimationType AnimationType => animationType;
    public float Duration => duration;
    public float Strength => strength;
    public bool IsLoop => loop;

    private void Awake()
    {
        // 초기 Transform 값 저장
        _initialPosition = transform.localPosition;
        _initialRotation = transform.localEulerAngles;
        _initialScale = transform.localScale;
    }

    private void Start()
    {
        if (playOnStart && animationType != TweenAnimationType.NONE)
        {
            Play();
        }
    }

    private void OnDestroy()
    {
        Stop();
    }

    /// <summary>
    /// 현재 설정된 애니메이션 타입으로 재생
    /// </summary>
    public void Play()
    {
        Stop();
        PlayAnimation(animationType);
    }

    /// <summary>
    /// 애니메이션 정지 및 초기 상태로 복원
    /// </summary>
    public void Stop()
    {
        if (_currentTween != null && _currentTween.IsActive())
        {
            _currentTween.Kill();
            _currentTween = null;
        }

        // 초기 상태로 복원
        transform.localPosition = _initialPosition;
        transform.localEulerAngles = _initialRotation;
        transform.localScale = _initialScale;
    }

    /// <summary>
    /// 애니메이션 타입 변경
    /// </summary>
    public void SetAnimationType(TweenAnimationType type)
    {
        animationType = type;
    }

    private void PlayAnimation(TweenAnimationType type)
    {
        var loopType = loop ? LoopType.Yoyo : LoopType.Restart;
        var loopCount = loop ? -1 : 0;

        switch (type)
        {
            case TweenAnimationType.BOBBING:
                PlayBobbing(loopCount);
                break;
            case TweenAnimationType.SHAKE:
                PlayShake(loopCount);
                break;
            case TweenAnimationType.ROTATE:
                PlayRotate();
                break;
            case TweenAnimationType.SCALE_PULSE:
                PlayScalePulse(loopCount);
                break;
            case TweenAnimationType.SWING:
                PlaySwing(loopCount);
                break;
        }
    }

    // 위아래로 왔다갔다하는 애니메이션
    private void PlayBobbing(int loopCount)
    {
        var targetPos = _initialPosition + Vector3.up * (bobbingHeight * strength);
        _currentTween = transform
            .DOLocalMove(targetPos, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(loopCount, LoopType.Yoyo);
    }

    // 흔들리는 애니메이션
    private void PlayShake(int loopCount)
    {
        /*
        Shake는 자체적으로 반복이 안되므로
        OnComplete 콜백으로 재귀 호출
        */
        _currentTween = transform
            .DOShakePosition(duration, strength, shakeVibrato, shakeRandomness)
            .OnComplete(() =>
            {
                if (loop && this != null && gameObject.activeInHierarchy)
                {
                    transform.localPosition = _initialPosition;
                    PlayShake(loopCount);
                }
            });
    }

    // 지속적으로 회전하는 애니메이션
    private void PlayRotate()
    {
        var rotateAmount = rotateAxis.normalized * (360f * strength);
        _currentTween = transform
            .DOLocalRotate(rotateAmount, duration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(loop ? -1 : 0, LoopType.Restart);
    }

    // 크기가 커졌다 작아졌다 하는 애니메이션
    private void PlayScalePulse(int loopCount)
    {
        var targetScale = _initialScale * (scaleMultiplier * strength);
        _currentTween = transform
            .DOScale(targetScale, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(loopCount, LoopType.Yoyo);
    }

    // 좌우로 흔들리는 시계추 애니메이션
    private void PlaySwing(int loopCount)
    {
        // 먼저 한쪽으로 기울인 후 반복
        var angle = swingAngle * strength;
        transform.localEulerAngles = _initialRotation + Vector3.forward * angle;

        _currentTween = transform
            .DOLocalRotate(_initialRotation + Vector3.forward * -angle, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(loopCount, LoopType.Yoyo);
    }
}
