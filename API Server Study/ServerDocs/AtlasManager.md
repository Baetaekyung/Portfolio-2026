# AtlasManager

## 개요
Unity SpriteAtlas를 활용하여 이름으로 스프라이트를 가져오는 매니저입니다.
싱글톤 패턴으로 구현되어 전역에서 접근 가능합니다.

## 주요 기능

### 1. 아틀라스 등록
- `void RegisterAtlas(SpriteAtlas atlas)` : 아틀라스를 매니저에 등록합니다.
- `void RegisterAtlas(string key, SpriteAtlas atlas)` : 키와 함께 아틀라스를 등록합니다.

### 2. 스프라이트 가져오기
- `Sprite GetSprite(string spriteName)` : 등록된 모든 아틀라스에서 이름으로 스프라이트를 검색합니다.
- `Sprite GetSprite(string atlasKey, string spriteName)` : 특정 아틀라스에서 스프라이트를 가져옵니다.

### 3. 아틀라스 해제
- `void UnregisterAtlas(string key)` : 특정 아틀라스를 해제합니다.
- `void Clear()` : 모든 등록된 아틀라스를 해제합니다.

## 캐싱 정책
- 등록된 아틀라스는 Dictionary에 저장됩니다.
- 스프라이트 검색 결과는 캐싱하여 중복 검색을 방지합니다.

## 사용 예시

```csharp
// 아틀라스 등록
AtlasManager.Instance.RegisterAtlas("UI", uiAtlas);

// 스프라이트 가져오기 (특정 아틀라스에서)
var icon = AtlasManager.Instance.GetSprite("UI", "icon_gold");

// 스프라이트 가져오기 (전체 검색)
var sprite = AtlasManager.Instance.GetSprite("btn_confirm");

// 아틀라스 해제
AtlasManager.Instance.UnregisterAtlas("UI");

// 모든 아틀라스 해제
AtlasManager.Instance.Clear();
```

## 의존성
- Unity 2D Sprite 패키지 (SpriteAtlas)
