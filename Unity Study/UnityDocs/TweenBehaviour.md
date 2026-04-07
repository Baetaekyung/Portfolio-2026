# TweenBehaviour

Tween을 이용한 지속적 애니메이션을 제공하는 컴포넌트입니다.

## 개요

자주 사용되는 Tween 애니메이션들을 Enum으로 분류하여 Inspector에서 쉽게 선택하고 적용할 수 있도록 합니다.

## 애니메이션 타입 (TweenAnimationType)

| Enum | 설명 |
|------|------|
| NONE | 애니메이션 없음 |
| BOBBING | 위아래로 왔다갔다 (부유하는 효과) |
| SHAKE | 흔들리는 효과 |
| ROTATE | 지속적으로 회전 |
| SCALE_PULSE | 크기가 커졌다 작아졌다 (맥박 효과) |
| SWING | 좌우로 흔들림 (시계추 효과) |

## SerializeField 변수

| 변수명 | 타입 | 설명 |
|--------|------|------|
| animationType | TweenAnimationType | 애니메이션 타입 선택 |
| duration | float | 애니메이션 한 사이클 시간 |
| strength | float | 애니메이션 강도 |
| playOnStart | bool | 시작 시 자동 재생 여부 |
| loop | bool | 반복 여부 |

## Public 메서드

| 메서드 | 설명 |
|--------|------|
| Play() | 애니메이션 재생 |
| Stop() | 애니메이션 정지 |
| SetAnimationType(TweenAnimationType type) | 애니메이션 타입 변경 |

## 모니터링용 프로퍼티 (읽기 전용)

| 프로퍼티 | 타입 | 설명 |
|----------|------|------|
| IsPlaying | bool | 현재 Tween이 재생 중인지 여부 |
| AnimationType | TweenAnimationType | 현재 설정된 애니메이션 타입 |
| Duration | float | 애니메이션 한 사이클 시간 |
| Strength | float | 애니메이션 강도 |
| IsLoop | bool | 반복 설정 여부 |

## 사용 예시

```csharp
// Inspector에서 설정하거나 코드로 제어
var tweenBehaviour = GetComponent<TweenBehaviour>();
tweenBehaviour.SetAnimationType(TweenAnimationType.BOBBING);
tweenBehaviour.Play();
```

## 의존성

- DOTween (DG.Tweening)
