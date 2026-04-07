# SafeArea

모바일 디바이스의 SafeArea를 적용하는 컴포넌트입니다.

## 파일 위치

`Assets/Script/Common/Utils/SafeArea.cs`

## 기능

- 노치, 홈 인디케이터 등을 피해 UI를 안전하게 배치
- 화면 회전 시 자동으로 SafeArea 재적용
- 방향별 개별 적용 설정 가능

## 사용 방법

1. SafeArea를 적용할 UI 패널(RectTransform)에 컴포넌트 추가
2. Inspector에서 적용할 방향 설정
3. 실행 시 자동으로 SafeArea 적용

## Inspector 설정

| 항목 | 설명 |
|------|------|
| Apply Top | 상단 SafeArea 적용 (노치 영역) |
| Apply Bottom | 하단 SafeArea 적용 (홈 인디케이터) |
| Apply Left | 좌측 SafeArea 적용 |
| Apply Right | 우측 SafeArea 적용 |

## 권장 구조

```
Canvas
└── SafeAreaPanel (SafeArea 컴포넌트 추가)
    └── UI 콘텐츠들
```

## 주의사항

- RectTransform이 필요하므로 UI 오브젝트에만 사용
- 부모 Canvas의 Render Mode가 Screen Space일 때 정상 작동
- 에디터에서는 Device Simulator로 테스트 권장

## Context Menu

에디터에서 컴포넌트 우클릭 시 사용 가능:

- **Apply SafeArea (Editor)**: 에디터에서 SafeArea 강제 적용
- **Reset SafeArea**: SafeArea 초기화 (전체 화면으로 복원)
