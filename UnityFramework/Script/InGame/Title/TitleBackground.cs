using UnityEngine;
using UnityEngine.UI;

public class TitleBackground : MonoBehaviour
{
    [SerializeField] private RectTransform[] backgrounds = new RectTransform[2];
    [SerializeField] private float scrollSpeed = 100f;

    private float _backgroundWidth;

    private void Start()
    {
        // 배경 이미지의 너비 계산
        if (backgrounds[0] != null)
        {
            _backgroundWidth = backgrounds[0].rect.width;
        }
    }

    private void Update()
    {
        ScrollBackgrounds();
    }

    // 배경 스크롤 처리
    private void ScrollBackgrounds()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] == null) continue;

            // -x 방향으로 이동
            backgrounds[i].anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

            // 화면 밖으로 나가면 오른쪽 끝으로 재배치
            if (backgrounds[i].anchoredPosition.x <= -_backgroundWidth)
            {
                var newPos = backgrounds[i].anchoredPosition;
                newPos.x += _backgroundWidth * 2;
                backgrounds[i].anchoredPosition = newPos;
            }
        }
    }
}
