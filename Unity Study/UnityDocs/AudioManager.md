# AudioManager

## 개요
GameObjectPool 기반의 오디오 관리 시스템입니다.
SoundObject를 풀링하여 SFX와 BGM을 효율적으로 재생합니다.

## 클래스 정보
- **위치**: `Assets/Script/Common/Audio/`
- **주요 클래스**: `AudioManager`, `SoundObject`
- **의존성**: `GameObjectPool`, `AssetLoader`

## 아키텍처

```
AudioManager (싱글톤)
├── BGM Channel (단일)
│   └── 현재 재생 중인 BGM
└── SFX Pool (다중)
    ├── SoundObject - Playing
    ├── SoundObject - Playing
    └── SoundObject - Pooled
```

## SoundObject

AudioSource를 래핑한 풀링 가능한 오브젝트입니다.

### 주요 기능
- IPoolable 구현으로 Spawn/Despawn 시 자동 초기화
- 재생 완료 시 자동 풀 반환
- Loop 재생 지원

### 구조
```csharp
public class SoundObject : MonoBehaviour, GameObjectPool.IPoolable
{
    public AudioSource AudioSource { get; }

    public void Play(AudioClip clip, float volume, bool loop);
    public void Stop();
}
```

## AudioManager API

### BGM
| 메서드 | 설명 |
|--------|------|
| `PlayBGM(key, volume, fadeIn)` | BGM 재생 (기존 BGM 정지) |
| `StopBGM(fadeOut)` | BGM 정지 |
| `PauseBGM()` | BGM 일시정지 |
| `ResumeBGM()` | BGM 재개 |
| `SetBGMVolume(volume)` | BGM 볼륨 설정 |

### SFX
| 메서드 | 설명 |
|--------|------|
| `PlaySFX(key, volume)` | SFX OneShot 재생 |
| `PlaySFXLoop(key, volume)` | SFX Loop 재생 (핸들 반환) |
| `StopSFX(handle)` | 특정 SFX 정지 |
| `StopAllSFX()` | 모든 SFX 정지 |
| `SetSFXVolume(volume)` | SFX 마스터 볼륨 설정 |

### 전역
| 메서드 | 설명 |
|--------|------|
| `SetMasterVolume(volume)` | 전체 볼륨 설정 |
| `Mute(bool)` | 전체 음소거 |
| `StopAll()` | 모든 사운드 정지 |

## 사용 예시

### BGM 재생
```csharp
// BGM 재생 (기존 BGM 자동 정지)
AudioManager.Instance.PlayBGM("BGM/MainTheme");

// 페이드 인/아웃과 함께 재생
AudioManager.Instance.PlayBGM("BGM/BattleTheme", fadeIn: 1f);
AudioManager.Instance.StopBGM(fadeOut: 0.5f);
```

### SFX 재생
```csharp
// OneShot (재생 후 자동 반환)
AudioManager.Instance.PlaySFX("SFX/Click");
AudioManager.Instance.PlaySFX("SFX/Explosion", volume: 0.8f);

// Loop (수동 정지 필요)
var handle = AudioManager.Instance.PlaySFXLoop("SFX/Engine");
// ... 나중에
AudioManager.Instance.StopSFX(handle);
```

### 볼륨 조절
```csharp
// 개별 볼륨
AudioManager.Instance.SetBGMVolume(0.7f);
AudioManager.Instance.SetSFXVolume(1.0f);

// 마스터 볼륨
AudioManager.Instance.SetMasterVolume(0.5f);

// 음소거
AudioManager.Instance.Mute(true);
```

## 설정

### Inspector
```csharp
[Header("볼륨 설정")]
[SerializeField, Range(0, 1)] private float masterVolume = 1f;
[SerializeField, Range(0, 1)] private float bgmVolume = 1f;
[SerializeField, Range(0, 1)] private float sfxVolume = 1f;

[Header("풀 설정")]
[SerializeField] private string soundObjectKey = "Audio/SoundObject";
[SerializeField] private int initialPoolSize = 10;
```

## 주의사항

- BGM은 항상 단일 채널로 재생 (새 BGM 재생 시 기존 자동 정지)
- SFX Loop는 반드시 StopSFX로 정지해야 함
- Addressables 키로 AudioClip 로드
- SoundObject 프리팹은 AudioSource 컴포넌트 필수
